using DocumentFormat.OpenXml.Vml.Office;
using Spix.Domain.Entities;
using Spix.Domain.EntitiesSchedule;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.EnumMultilLanguage;
using Spix.AppInfra.Extensions;
using Spix.AppInfra.Mappings;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppService.InterfaceContratos;
using Spix.Domain.EntitiesContratos;
using Spix.DomainLogic.EntitiesContractDTO;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ItemsGeneric;
using Spix.DomainLogic.ModelUtility;
using Spix.DomainLogic.Pagination;
using Spix.xLanguage.Resources;

namespace Spix.AppService.ImplementContratos
{
    public class ContractClientService : IContractClientService
    {
        private readonly DataContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ITransactionManager _transactionManager;
        private readonly IUserHelper _userHelper;
        private readonly IMapperService _mapperService;
        private readonly HttpErrorHandler _httpErrorHandler;
        private readonly IEnumMultilLanguageService _enumMultilLanguageService;
        private readonly IContractActivationIntegrityService _contractActivationIntegrityService;

        public ContractClientService(DataContext context, IHttpContextAccessor httpContextAccessor,
            ITransactionManager transactionManager, IUserHelper userHelper, IMapperService mapperService,
            HttpErrorHandler httpErrorHandler, IEnumMultilLanguageService enumMultilLanguageService,
            IContractActivationIntegrityService contractActivationIntegrityService)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
            _transactionManager = transactionManager;
            _userHelper = userHelper;
            _mapperService = mapperService;
            _httpErrorHandler = httpErrorHandler;
            _enumMultilLanguageService = enumMultilLanguageService;
            _contractActivationIntegrityService = contractActivationIntegrityService;
        }
        public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetComboStatusAsync()
        {
            try
            {
                List<IntItemModel> list = _enumMultilLanguageService.GetEnumSelectList<ContractState>(nameof(Resource.Select_Status));
                int[] contractStateOrder =
                {
                    (int)ContractState.Draft,
                    (int)ContractState.PendingApproval,
                    (int)ContractState.InProgress,
                    (int)ContractState.Active,
                    (int)ContractState.Exempt,
                    (int)ContractState.Suspended,
                    (int)ContractState.Cancelled,
                    (int)ContractState.Terminated
                };

                list = list
                    .OrderBy(x => Array.IndexOf(contractStateOrder, x.Value))
                    .ToList();

                return new ActionResponse<IEnumerable<IntItemModel>>
                {
                    WasSuccess = true,
                    Result = list
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
            }
        }

        public async Task<ActionResponse<IEnumerable<IntItemModel>>> GetContractClientComboStatusAsync()
        {
            try
            {
                int[] contractClientStates =
                {
                    (int)ContractState.Draft,
                    (int)ContractState.PendingApproval,
                    (int)ContractState.InProgress
                };

                //Igual que CorporationService.ComboAsync: el backend entrega la lista lista para pintar,
                //con el elemento neutro traducido en la primera posicion. El front no filtra nada.
                List<IntItemModel> todos = _enumMultilLanguageService
                    .GetEnumSelectList<ContractState>(nameof(Resource.Select_Status));

                IntItemModel neutro = todos.First();

                List<IntItemModel> list = todos
                    .Where(x => contractClientStates.Contains(x.Value))
                    .OrderBy(x => Array.IndexOf(contractClientStates, x.Value))
                    .ToList();

                list.Insert(0, neutro);

                return new ActionResponse<IEnumerable<IntItemModel>>
                {
                    WasSuccess = true,
                    Result = list
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<IntItemModel>>(ex);
            }
        }

        public async Task<ActionResponse<IEnumerable<ContractClient>>> GetAsync(PaginationDTO pagination, string username)
        {
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    return new ActionResponse<IEnumerable<ContractClient>>
                    {
                        WasSuccess = false,
                        Message = "Problemas de Validacion de Usuario"
                    };
                }

                //AsNoTracking es obligatorio aqui: sin el, EF rellena las colecciones inversas
                //(Client/Contractor/Zone/EstratoSocial.ContractClients) con las demas filas y el JSON
                //crece de forma explosiva (IgnoreCycles no corta entre filas hermanas). Con pocos
                //contratos el navegador llegaba a congelarse deserializando la respuesta.
                var queryable = _context.ContractClients.AsNoTracking()
                    .Include(x => x.Client)
                    .Include(x => x.Contractor)
                    .Include(x => x.Zone)
                    .Include(x => x.EstratoSocial)
                    .Include(c => c.ContractIDPic)
                    .Where(x => x.CorporationId == user.CorporationId)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(pagination.Filter))
                {
                    var filter = pagination.Filter.Trim();
                    queryable = queryable.Where(u =>
                        EF.Functions.Like(u.Client!.FirstName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client!.LastName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client!.FirstName + " " + u.Client!.LastName, $"%{filter}%") ||
                        EF.Functions.Like(u.Client.Document, $"%{filter}%"));
                }

                //Filtro por estado desde el dropdown del listado (pagination.Id = ContractState; 0 = todos)
                if (pagination.Id > 0 && Enum.IsDefined(typeof(ContractState), pagination.Id))
                {
                    var state = (ContractState)pagination.Id;
                    queryable = queryable.Where(x => x.ContractState == state);
                }

                await _httpContextAccessor.HttpContext!.InsertParameterPagination(queryable, pagination.RecordsNumber);

                //Orden del trabajo: primero lo que hay que atender (Draft), luego lo que espera
                //aprobacion, luego lo que se esta configurando, y de ultimo el resto.
                //Dentro de cada estado, del mas nuevo al mas viejo.
                var modelo = await queryable
                    .OrderBy(x => x.ContractState == ContractState.Draft ? 0
                                : x.ContractState == ContractState.PendingApproval ? 1
                                : x.ContractState == ContractState.InProgress ? 2
                                : 3)
                    .ThenByDescending(x => x.DateCreado)
                    .ThenByDescending(x => x.ControlContrato)
                    .Paginate(pagination)
                    .ToListAsync();

                //Marca los que ya tienen fotos y firmas (el listado muestra el boton Aprobar)
                var ids = modelo.Select(x => x.ContractClientId).ToList();
                var completeIds = await ContractRequirementRules.GetCompleteIdsAsync(_context, ids);

                //Y los que ya firmaron los dos documentos (ahi se esconde el boton de enviar la solicitud)
                var signedIds = await ContractRequirementRules.GetSignedIdsAsync(_context, ids);

                foreach (var item in modelo)
                {
                    item.RequirementsComplete = completeIds.Contains(item.ContractClientId);
                    item.SignaturesComplete = signedIds.Contains(item.ContractClientId);
                }

                return new ActionResponse<IEnumerable<ContractClient>>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<IEnumerable<ContractClient>>(ex);
            }
        }

        public async Task<ActionResponse<ContractClient>> GetAsync(Guid id)
        {
            try
            {
                var modelo = await _context.ContractClients.AsNoTracking()
                    .Include(x => x.Client)
                    .Include(x => x.Contractor)
                    .Include(c => c.ContractIDPic)

                    //El plan del contrato: de aqui salen PlanId y PlanCategoryId, que son
                    //[NotMapped] y por eso no vienen solos de la base. Sin esto, al abrir
                    //Editar los dos combos del plan salen vacios aunque este guardado.
                    .Include(x => x.ContractPlans!)
                        .ThenInclude(x => x.Plan)

                    .FirstOrDefaultAsync(x => x.ContractClientId == id);

                //La comprobacion va ANTES de tocar el modelo: estaba despues, asi que un
                //id inexistente reventaba con una nula en vez de dar el mensaje.
                if (modelo == null)
                {
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Enconstrar el Registro Indicado"
                    };
                }

                //El estado y la ciudad se deducen de la zona: tambien son [NotMapped]
                var ZoneDetail = await _context.Zones.AsNoTracking()
                    .FirstOrDefaultAsync(x => x.ZoneId == modelo.ZoneId);

                if (ZoneDetail != null)
                {
                    modelo.StateId = ZoneDetail.StateId;
                    modelo.CityId = ZoneDetail.CityId;
                }

                var planDelContrato = modelo.ContractPlans?.FirstOrDefault()?.Plan;
                if (planDelContrato != null)
                {
                    modelo.PlanId = planDelContrato.PlanId;
                    modelo.PlanCategoryId = planDelContrato.PlanCategoryId;
                }

                return new ActionResponse<ContractClient>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
            }
        }

        public async Task<ActionResponse<ContractClient>> UpdateAsync(ContractClient modelo)
        {
            await _transactionManager.BeginTransactionAsync();

            try
            {
                var currentContract = await _context.ContractClients
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.ContractClientId,
                        x.ContractState,
                        x.CorporationId
                    })
                    .FirstOrDefaultAsync(x => x.ContractClientId == modelo.ContractClientId);

                bool isChangingContractState = currentContract != null &&
                    currentContract.ContractState != modelo.ContractState;

                if (isChangingContractState &&
                    modelo.ContractState == ContractState.Active)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Result = modelo,
                        Message = "Para activar el contrato debe finalizar su configuracion desde ContractControl."
                    };
                }

                //In Progress solo con fotos del documento, Consentimiento y Contrato firmados
                if (isChangingContractState &&
                    modelo.ContractState == ContractState.InProgress)
                {
                    var missing = await ContractRequirementRules.GetMissingAsync(_context, modelo.ContractClientId);
                    if (missing.Count > 0)
                    {
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<ContractClient>
                        {
                            WasSuccess = false,
                            Result = modelo,
                            Message = $"Para pasar a In Progress falta: {string.Join(", ", missing)}."
                        };
                    }
                }

                bool requiresMikrotikValidation = modelo.ContractState == ContractState.Suspended;

                if (isChangingContractState &&
                    requiresMikrotikValidation &&
                    currentContract is not null)
                {
                    var integrityResponse = await _contractActivationIntegrityService.ValidateAsync(
                        modelo.ContractClientId);
                    if (!integrityResponse.WasSuccess)
                    {
                        await _transactionManager.RollbackTransactionAsync();
                        return new ActionResponse<ContractClient>
                        {
                            WasSuccess = false,
                            Result = modelo,
                            Message = integrityResponse.Message
                        };
                    }
                }

                //Se asigna campo por campo sobre la fila que ya existe.
                //
                //Antes era Update(modelo), que marca TODAS las columnas como modificadas y
                //escribe la entidad completa. Pero el formulario no manda la entidad
                //completa: arma un objeto con una lista escogida a mano. Todo lo que no
                //viajaba se escribia en NULL, y con ello se borraban:
                //
                //  - SignatureRequestedAt: el contrato desaparecia del portal del cliente
                //    aunque la solicitud de firma ya se hubiera enviado
                //  - UsuarioOwner y UserId: quien creo el contrato
                //
                //Bastaba con abrir Editar y guardar para perderlos.
                var current = await _context.ContractClients
                    .FirstOrDefaultAsync(x => x.ContractClientId == modelo.ContractClientId);

                if (current == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Enconstrar el Registro Indicado"
                    };
                }

                //Lo que el formulario edita, y nada mas. Ni la corporacion, ni el
                //consecutivo, ni la fecha de creacion: eso no se toca al editar.
                current.ContractorId = modelo.ContractorId;
                current.ClientId = modelo.ClientId;
                //El telefono viaja partido: sin los indicativos el numero queda a medias
                current.CodeCountry = modelo.CodeCountry;
                current.CodeNumber = modelo.CodeNumber;
                current.PhoneNumber = modelo.PhoneNumber;
                current.CodeCountry2 = modelo.CodeCountry2;
                current.CodeNumber2 = modelo.CodeNumber2;
                current.PhoneNumber2 = modelo.PhoneNumber2;
                current.Address = modelo.Address;
                current.ZoneId = modelo.ZoneId;
                current.ContractState = modelo.ContractState;
                current.EquipoEmpres = modelo.EquipoEmpres;
                current.EnvoiceClient = modelo.EnvoiceClient;
                current.EstratoSocialId = modelo.EstratoSocialId;

                //El plan: el contrato tiene exactamente uno. Si el formulario mando otro,
                //se actualiza la fila; si todavia no existe, se crea.
                if (modelo.PlanId != Guid.Empty)
                {
                    var contractPlan = await _context.ContractPlans
                        .FirstOrDefaultAsync(x => x.ContractClientId == current.ContractClientId);

                    if (contractPlan == null)
                    {
                        _context.ContractPlans.Add(new ContractPlan
                        {
                            ContractClientId = current.ContractClientId,
                            PlanId = modelo.PlanId
                        });
                    }
                    else if (contractPlan.PlanId != modelo.PlanId)
                    {
                        contractPlan.PlanId = modelo.PlanId;
                    }
                }

                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<ContractClient>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
            }
        }

        public async Task<ActionResponse<bool>> ApproveAsync(Guid id, string username)
        {
            await _transactionManager.BeginTransactionAsync();
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Problemas de Validacion de Usuario"
                    };
                }

                var contract = await _context.ContractClients
                    .FirstOrDefaultAsync(x => x.ContractClientId == id && x.CorporationId == user.CorporationId);

                if (contract == null)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Enconstrar el Registro Indicado"
                    };
                }

                //Solo se aprueba lo que esta en Pending Approval y tiene todo completo
                if (contract.ContractState != ContractState.PendingApproval)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Solo se pueden aprobar contratos en Pending Approval."
                    };
                }

                var missing = await ContractRequirementRules.GetMissingAsync(_context, id);
                if (missing.Count > 0)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = $"Para pasar a In Progress falta: {string.Join(", ", missing)}."
                    };
                }

                contract.ContractState = ContractState.InProgress;

                //Al aprobar nace la visita para instalar el servicio. Va en la MISMA
                //transaccion que el cambio de estado: si algo falla, ni el contrato
                //avanza ni queda una instalacion huerfana.
                await CrearInstalacionAsync(contract, user);

                await ContractAuditLog.AddAsync(_context, contract.ContractClientId, ContractEventType.Approved,
                    ContractState.InProgress.ToString(), $"{user.FirstName} {user.LastName}".Trim(),
                    Guid.TryParse(user.Id, out var approveUserId) ? approveUserId : null,
                    clientId: contract.ClientId, corporationId: contract.CorporationId);

                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
            }
        }

        // La visita de instalacion que nace al aprobar el contrato.
        //
        // Es la misma solicitud de servicio de siempre, con dos diferencias: el origen
        // queda en Installation (asi se puede filtrar y reportar aunque despues se
        // complete) y el estado en Requested, que significa "ya esta pedida pero la
        // oficina todavia no le asigna tecnico ni fecha". Desde ahi sigue el flujo
        // normal: la revisan, la agendan y la cierran.
        //
        // Los datos del contrato se copian CONGELADOS, igual que en una solicitud
        // normal: si mañana cambian la direccion o el plan, la visita sigue diciendo
        // con que datos se mando al tecnico.
        private async Task CrearInstalacionAsync(ContractClient contract, User user)
        {
            //Si ya tiene una instalacion viva no se crea otra: aprobar dos veces no
            //puede mandar al tecnico dos veces al mismo sitio.
            var yaTiene = await _context.ServiceRequests.AnyAsync(x =>
                x.ContractClientId == contract.ContractClientId &&
                x.Origin == ServiceRequestOrigin.Installation &&
                x.ScheduleStatus != ScheduleStatus.Cancelled);

            if (yaTiene)
            {
                return;
            }

            //Los datos del contrato para copiarlos a la visita
            var datos = await _context.ContractClients.AsNoTracking()
                .Where(x => x.ContractClientId == contract.ContractClientId)
                .Select(x => new
                {
                    Cliente = x.Client!.FirstName + " " + x.Client.LastName,
                    x.CodeCountry,
                    x.CodeNumber,
                    x.PhoneNumber,
                    x.Address,
                    Ciudad = x.Zone!.City!.Name,
                    Zona = x.Zone!.ZoneName,
                    Servidor = x.ContractServers!.Select(s => s.Server!.ServerName).FirstOrDefault(),
                    IpServidor = x.ContractServers!.Select(s => s.Server!.IpNetwork!.Ip).FirstOrDefault(),
                    IpCliente = x.ContractIps!.Select(i => i.IpNet!.Ip).FirstOrDefault(),
                    Mac = x.ContractMacs!.Select(m => m.CargueDetail!.MacWlan).FirstOrDefault(),
                    Plan = x.ContractPlans!.Select(p => p.Plan!.PlanName).FirstOrDefault(),
                    Nodo = x.ContractNodes!.Select(n => n.Node!.NodesName).FirstOrDefault(),
                    IpNodo = x.ContractNodes!.Select(n => n.Node!.IpNetwork!.Ip).FirstOrDefault(),

                    //La ubicacion registrada, para que el tecnico sepa a donde ir.
                    //Si el contrato todavia no la tiene, la visita nace sin ella y la
                    //captura el tecnico cuando llegue.
                    Latitud = x.ContractMaps!.Select(m => m.Latitude).FirstOrDefault(),
                    Longitud = x.ContractMaps!.Select(m => m.Longitude).FirstOrDefault()
                })
                .FirstOrDefaultAsync();

            if (datos is null)
            {
                return;
            }

            var siguiente = await _context.ServiceRequests
                .Where(x => x.CorporationId == contract.CorporationId)
                .Select(x => (long?)x.RequestNumber)
                .MaxAsync() ?? 0;

            var telefono = PhoneHelper.Visible(datos.CodeCountry, datos.CodeNumber, datos.PhoneNumber);

            _context.ServiceRequests.Add(new ServiceRequest
            {
                RequestNumber = siguiente + 1,
                CreatedAtUtc = DateTime.UtcNow,
                ContractClientId = contract.ContractClientId,

                //Sin tecnico ni fecha: eso lo pone la oficina al revisarla
                ScheduleStatus = ScheduleStatus.Requested,
                Origin = ServiceRequestOrigin.Installation,

                ClientReason = "Instalacion del servicio",
                CorporationId = contract.CorporationId,
                UserId = Guid.TryParse(user.Id, out var id) ? id : Guid.Empty,
                UsuarioOwner = $"{user.FirstName} {user.LastName}".Trim(),

                ControlContrato = contract.ControlContrato,
                ClientFullName = datos.Cliente,
                PhoneNumber = telefono,
                ContactPhone = telefono,
                Address = datos.Address,
                CityName = datos.Ciudad,
                ZoneName = datos.Zona,
                ServerName = datos.Servidor,
                IpServer = datos.IpServidor,
                IpCliente = datos.IpCliente,
                MacCliente = datos.Mac,
                PlanName = datos.Plan,
                NodeName = datos.Nodo,
                NodeIp = datos.IpNodo,

                //A donde tiene que ir. La coordenada propia de la visita (donde estuvo
                //de verdad) la pone el tecnico desde la app.
                Latitude = datos.Latitud,
                Longitude = datos.Longitud
            });
        }

        public async Task<ActionResponse<ContractClient>> AddAsync(ContractClient modelo, string username)
        {
            await _transactionManager.BeginTransactionAsync();
            try
            {
                var user = await _userHelper.GetUserByUserNameAsync(username);
                if (user == null)
                {
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Problemas de Validacion de Usuario"
                    };
                }

                //El plan es obligatorio y tiene que ser de SU corporacion: llega por el cuerpo
                //de la peticion, asi que no alcanza con que el combo del front lo haya filtrado
                if (modelo.PlanId == Guid.Empty)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "Debe seleccionar el plan del contrato."
                    };
                }

                var planOk = await _context.Plans.AnyAsync(x =>
                    x.PlanId == modelo.PlanId &&
                    x.CorporationId == user.CorporationId);

                if (!planOk)
                {
                    await _transactionManager.RollbackTransactionAsync();
                    return new ActionResponse<ContractClient>
                    {
                        WasSuccess = false,
                        Message = "El plan seleccionado no existe o no es de su empresa."
                    };
                }

                //Para crear el correlativo de Contratos
                var lastNumber = await _context.ContractClients.AsNoTracking()
                    .Where(x => x.CorporationId == user.CorporationId)
                                    .MaxAsync(x => (long?)x.ControlContrato) ?? 0;
                modelo.ControlContrato = lastNumber + 1;

                modelo.CorporationId = Convert.ToInt32(user.CorporationId);
                modelo.ContractState = ContractState.Draft;
                modelo.DateCreado = DateTime.Now;
                //control de Auditoria
                modelo.UsuarioOwner = $"{user.FirstName!} {user.LastName!}";
                modelo.UserId = Guid.Parse(user.Id);

                _context.ContractClients.Add(modelo);

                //Se guarda AQUI, antes de colgarle hijos.
                //
                //La llave del contrato la genera la base (NEWSEQUENTIALID), asi que hasta
                //este punto ContractClientId esta vacio. Tanto la bitacora como el plan la
                //reciben como ESCALAR, y a un escalar EF no se la corrige: se irian con
                //una llave foranea vacia y el guardado revienta.
                //
                //Sigue todo en la MISMA transaccion, asi que o entra completo o no entra.
                await _transactionManager.SaveChangesAsync();

                //Su plan: un contrato nunca queda sin plan. Todo lo que ya leia ContractPlan
                //(el queue padre, los reportes) sigue igual, solo que la fila nace al crear
                //el contrato y no despues, en el detalle.
                _context.ContractPlans.Add(new ContractPlan
                {
                    ContractClientId = modelo.ContractClientId,
                    PlanId = modelo.PlanId
                });

                //Primer renglon de la bitacora del contrato
                await ContractAuditLog.AddAsync(_context, modelo.ContractClientId, ContractEventType.Created,
                    $"Contrato {modelo.ControlContrato}", modelo.UsuarioOwner, modelo.UserId,
                    clientId: modelo.ClientId, corporationId: modelo.CorporationId);

                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<ContractClient>
                {
                    WasSuccess = true,
                    Result = modelo
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<ContractClient>(ex);
            }
        }

        public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
        {
            await _transactionManager.BeginTransactionAsync();
            try
            {
                var DataRemove = await _context.ContractClients.FindAsync(id);
                if (DataRemove == null)
                {
                    return new ActionResponse<bool>
                    {
                        WasSuccess = false,
                        Message = "Problemas para Enconstrar el Registro Indicado"
                    };
                }
                _context.ContractClients.Remove(DataRemove);

                await _transactionManager.SaveChangesAsync();
                await _transactionManager.CommitTransactionAsync();

                return new ActionResponse<bool>
                {
                    WasSuccess = true,
                    Result = true
                };
            }
            catch (Exception ex)
            {
                await _transactionManager.RollbackTransactionAsync();
                return await _httpErrorHandler.HandleErrorAsync<bool>(ex);
            }
        }
    }
}



