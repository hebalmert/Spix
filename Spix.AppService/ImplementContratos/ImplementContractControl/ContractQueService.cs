using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Spix.AppInfra;
using Spix.AppInfra.ErrorHandling;
using Spix.AppInfra.Transactions;
using Spix.AppInfra.UserHelper;
using Spix.AppInfra.Validations;
using Spix.AppService.InterfaceContratos.InterfaceContractControl;
using Spix.Domain.EntitiesContratos;
using Spix.Domain.EntitiesMK;
using Spix.Domain.EntitiesOper;
using Spix.DomainLogic.EnumTypes;
using Spix.DomainLogic.ModelUtility;
using Spix.xLanguage.Resources;
using Spix.xNetwork.MkHelper;

namespace Spix.AppService.ImplementEntitiesNet;

public class ContractQueService : IContractQueService
{
    private readonly DataContext _context;
    private readonly ITransactionManager _transactionManager;
    private readonly IUserHelper _userHelper;
    private readonly IStringLocalizer _localizer;
    private readonly HttpErrorHandler _httpErrorHandler;

    public ContractQueService(DataContext context, ITransactionManager transactionManager,
        IUserHelper userHelper, IStringLocalizer localizer, HttpErrorHandler httpErrorHandler)
    {
        _context = context;
        _transactionManager = transactionManager;
        _userHelper = userHelper;
        _localizer = localizer;
        _httpErrorHandler = httpErrorHandler;
    }

    public async Task<ActionResponse<ContractQue>> GetAsync(Guid id)
    {
        if (id == Guid.Empty)
        {
            return new ActionResponse<ContractQue>
            {
                WasSuccess = false,
                Message = _localizer[nameof(Resource.Generic_InvalidId)]
            };
        }

        try
        {
            var modelo = await _context.ContractQues
                .Include(x => x.Server)
                .Include(x => x.IpNet)
                .Include(x => x.Plan)
                .FirstOrDefaultAsync(c => c.ContractClientId == id);

            return new ActionResponse<ContractQue>
            {
                WasSuccess = true,
                Result = modelo ?? new()
            };
        }
        catch (Exception ex)
        {
            return await _httpErrorHandler.HandleErrorAsync<ContractQue>(ex);
        }
    }

    public async Task<ActionResponse<ContractQue>> AddAsync(ContractQue modelo, string username)
    {
        if (!ValidatorModel.IsValid(modelo, out var errores))
        {
            return new ActionResponse<ContractQue>
            {
                WasSuccess = false,
                Result = modelo,
                Message = _localizer[nameof(Resource.Generic_InvalidModel)]
            };
        }

        await _transactionManager.BeginTransactionAsync();
        try
        {
            var user = await _userHelper.GetUserByUserNameAsync(username);
            if (user == null)
            {
                return new ActionResponse<ContractQue>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_AuthIdFail)]
                };
            }

            var exists = await _context.ContractQues.AnyAsync(x => x.ContractClientId == modelo.ContractClientId);
            if (exists)
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractQue>
                {
                    WasSuccess = false,
                    Message = "Ya existe una Queue de Velocidad para este contrato."
                };
            }

            _context.ContractQues.Add(modelo);
            await _transactionManager.SaveChangesAsync();

            var conContract = await _context.ContractClients
                .AsNoTracking()
                .Include(x => x.ContractPlans)
                .FirstOrDefaultAsync(x => x.ContractClientId == modelo.ContractClientId);
            var conCliente = await _context.Clients.
                FirstOrDefaultAsync(x => x.ClientId == conContract!.ClientId);
            var conServer = await _context.Servers
                .AsNoTracking()
                .Include(x => x.IpNetwork).FirstOrDefaultAsync(x => x.ServerId == modelo.ServerId);
            var conIpClient = await _context.IpNets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.IpNetId == modelo.IpNetId);
            var conPlan = await _context.Plans
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.PlanId == conContract!.ContractPlans!.FirstOrDefault()!.PlanId);
            var dato = new
            {
                nameserver = conServer!.ServerName,
                ipservidor = conServer.IpNetwork!.Ip,
                us = conServer.Usuario,
                pss = conServer.Clave,
                puerto = conServer.ApiPort,
                velocidad = $"{conPlan!.VelocidadUp}/{conPlan.VelocidadDown}",
                ipcliente = conIpClient!.Ip,
                nomCliente = $"{conCliente!.FirstName} {conCliente!.LastName} - ({conContract!.ControlContrato})",
                nomPlan = conPlan.PlanName
            };


            // Verificamos que en QueueParent no se haya creaco un Queu Paren para el plan nuevo
            var anyQueueParent = await _context.QueueParents.FirstOrDefaultAsync(x => x.ServerId == conServer.ServerId
            && x.PlanId == conPlan.PlanId);

            var conPlanList = await _context.ContractPlans.Where(x => x.PlanId == conPlan!.PlanId).ToListAsync();
            var CantCLientes = (from cn in _context.ContractClients
                                join pl in _context.ContractPlans on cn.ContractClientId equals pl.ContractClientId
                                join sv in _context.ContractServers on cn.ContractClientId equals sv.ContractClientId
                                join ip in _context.ContractIps on cn.ContractClientId equals ip.ContractClientId
                                join qu in _context.ContractQues on cn.ContractClientId equals qu.ContractClientId
                                where pl.PlanId == conPlan.PlanId && sv.ServerId == conServer.ServerId && qu.ContractClientId == cn.ContractClientId
                                select new
                                {
                                    ips = ip.IpNet!.Ip
                                }).ToList();
            int cantQueueClients = CantCLientes.Count;

            string ListaClientsIP = string.Empty;
            if (cantQueueClients == 0)
            {
                ListaClientsIP = dato.ipcliente!;
            }
            if (cantQueueClients > 0)
            {
                foreach (var item in CantCLientes)
                {
                    ListaClientsIP += item.ips + ", ";
                }
                ListaClientsIP = ListaClientsIP.TrimEnd(',', ' ');
                ListaClientsIP = $"{ListaClientsIP}, {dato.ipcliente}";
            }


            //Vamos a cargar en nombre del PCQ en las variables para poderlas agregar a la Mikrotik
            string PcqDown = string.Empty;
            string PcqUp = string.Empty;

            //calculamos todo en kbps para poder trabajar el Parent mejor
            int UpSpeed = 0;
            int DownSpeed = 0;
            UpSpeed = conPlan.SpeedUpType == SpeedUpType.M ? conPlan.SpeedUp * 1024 : conPlan.SpeedUp;
            DownSpeed = conPlan.SpeedDownType == SpeedDownType.M ? conPlan.SpeedDown * 1024 : conPlan.SpeedUp;
            int UpSpeedLimitAt = (UpSpeed / conPlan.TasaReuso);
            int DownSpeedLimitAt = (DownSpeed / conPlan.TasaReuso);
            int UpSpeedFather = cantQueueClients + 1 < conPlan.TasaReuso + 1 ? UpSpeed : (UpSpeedLimitAt * (cantQueueClients + 1));
            int DownSpeedFather = cantQueueClients + 1 < conPlan.TasaReuso + 1 ? DownSpeed : (DownSpeedLimitAt * (cantQueueClients + 1));


            var quetype = await _context.QueueTypes.Where(x => x.CorporationId == user!.CorporationId).ToListAsync();
            PcqDown = quetype.Where(x => x.Down == true).Select(x => x.TypeName).FirstOrDefault()!;
            PcqUp = quetype.Where(x => x.Up == true).Select(x => x.TypeName).FirstOrDefault()!;

            string nomQueues = $"{PcqUp}/{PcqDown}";
            string nomParent = $"Parent {dato.nomPlan} 1 a {conPlan.TasaReuso}";



            ////////////////////////////////////////////////////////////
            MK mikrotik = new MK(dato.ipservidor!, dato.puerto);
            if (!mikrotik.Login(dato.us, dato.pss))
            {
                await _transactionManager.RollbackTransactionAsync();
                return new ActionResponse<ContractQue>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }
            int total = 0;
            int rest = 0;
            string idmk;
            string mikrotiIndex = string.Empty;

            //Esta es para ver si el Parent Existe en el servidor
            if (anyQueueParent == null)
            {
                mikrotik.Send("/queue/simple/add");
                mikrotik.Send("=limit-at=" + $"{UpSpeedFather}k/{DownSpeedFather}k");
                mikrotik.Send("=max-limit=" + $"{UpSpeedFather}k/{DownSpeedFather}k");
                mikrotik.Send("=name=" + nomParent);
                mikrotik.Send("=queue=" + nomQueues);
                mikrotik.Send("=target=" + ListaClientsIP);
                mikrotik.Send("=priority=" + "5/5");
                mikrotik.Send("/queue/simple/print", true);
                foreach (var item in mikrotik.Read())
                {
                    idmk = item;
                    total = idmk.Length;
                    rest = total - 10;
                    mikrotiIndex = idmk.Substring(10, rest);
                }

                mikrotik.Send("/queue/simple/move");
                mikrotik.Send("=.id=" + mikrotiIndex);
                mikrotik.Send("=destination=1");
                mikrotik.Send("/queue/simple/print", true);
                int sum1 = 0;
                foreach (var item in mikrotik.Read())
                {
                    sum1 += 1;
                }

                QueueParent queueParent = new()
                {
                    CorporationId = Convert.ToInt32(user!.CorporationId),
                    ParentName = nomParent,
                    ServerId = conServer.ServerId,
                    PlanId = conPlan.PlanId,
                    Down = $"{DownSpeed}k",
                    Up = $"{UpSpeed}k",
                    MkId = mikrotiIndex
                };
                _context.Add(queueParent);
                await _context.SaveChangesAsync();

                mikrotik.Send("/queue/simple/add");
                mikrotik.Send("=limit-at=" + $"{UpSpeedLimitAt}k/{DownSpeedLimitAt}k");
                mikrotik.Send("=max-limit=" + $"{UpSpeed}k/{DownSpeed}k");
                mikrotik.Send("=name=" + dato.nomCliente);
                mikrotik.Send("=target=" + dato.ipcliente);
                mikrotik.Send("=parent=" + nomParent);
                mikrotik.Send("=priority=" + "5/5");
                mikrotik.Send("=queue=" + nomQueues);
                mikrotik.Send("/queue/simple/print", true);

                total = 0;
                rest = 0;
                idmk = string.Empty;
                mikrotiIndex = string.Empty;

                foreach (var item in mikrotik.Read())
                {
                    idmk = item;
                    total = idmk.Length;
                    rest = total - 10;
                    mikrotiIndex = idmk.Substring(10, rest);
                }

                mikrotik.Send("/queue/simple/move");
                mikrotik.Send("=.id=" + mikrotiIndex);
                mikrotik.Send("=destination=1");
                mikrotik.Send("/queue/simple/print", true);
                int sum = 0;
                foreach (var item in mikrotik.Read())
                {
                    sum += 1;
                }
            }
            else
            {
                mikrotik.Send("/queue/simple/set");
                mikrotik.Send("=.id=" + anyQueueParent.MkId);
                mikrotik.Send("=limit-at=" + $"{UpSpeedFather}k/{DownSpeedFather}k");
                mikrotik.Send("=max-limit=" + $"{UpSpeedFather}k/{DownSpeedFather}k");
                mikrotik.Send("=target=" + ListaClientsIP);
                mikrotik.Send("/queue/simple/print", true);
                foreach (var item in mikrotik.Read())
                {
                    idmk = item;
                    total = idmk.Length;
                    rest = total - 10;
                }

                anyQueueParent.Down = $"{DownSpeed}k";
                anyQueueParent.Up = $"{UpSpeed}k";

                _context.Update(anyQueueParent);
                await _context.SaveChangesAsync();

                mikrotik.Send("/queue/simple/add");
                mikrotik.Send("=limit-at=" + $"{UpSpeedLimitAt}k/{DownSpeedLimitAt}k");
                mikrotik.Send("=max-limit=" + $"{UpSpeed}k/{DownSpeed}k");
                mikrotik.Send("=name=" + dato.nomCliente);
                mikrotik.Send("=target=" + dato.ipcliente);
                mikrotik.Send("=parent=" + anyQueueParent.ParentName);
                mikrotik.Send("=priority=" + "5/5");
                mikrotik.Send("=queue=" + nomQueues);
                mikrotik.Send("/queue/simple/print", true);

                total = 0;
                rest = 0;
                idmk = string.Empty;
                mikrotiIndex = string.Empty;

                foreach (var item in mikrotik.Read())
                {
                    idmk = item;
                    total = idmk.Length;
                    rest = total - 10;
                    mikrotiIndex = idmk.Substring(10, rest);
                }

                mikrotik.Send("/queue/simple/move");
                mikrotik.Send("=.id=" + mikrotiIndex);
                mikrotik.Send("=destination=1");
                mikrotik.Send("/queue/simple/print", true);
                int sum = 0;
                foreach (var item in mikrotik.Read())
                {
                    sum += 1;
                }
            }

            mikrotik.Close();

            ContractQue modelo = new()
            {
                ContractId = Convert.ToInt32(idquecontract),
                ServerId = Convert.ToInt32(contratos.ContractServers!.FirstOrDefault()!.ServerId),
                PlanId = Convert.ToInt32(contratos.ContractPlans!.FirstOrDefault()!.PlanId),
                IpNetId = Convert.ToInt32(contratos.ContractIps!.FirstOrDefault()!.IpNetId),
                ServerName = contratos.ContractServers!.FirstOrDefault()!.Server!.ServerName,
                IpServer = contratos.ContractServers!.FirstOrDefault()!.Server!.IpNetwork!.Ip!,
                IpCliente = contratos.ContractIps!.FirstOrDefault()!.IpNet!.Ip!,
                PlanName = contratos.ContractPlans!.FirstOrDefault()!.Plan!.PlanName,
                TotalVelocidad = contratos.ContractPlans!.FirstOrDefault()!.Plan!.VelocidadTotal,
                MikrotikId = mikrotiIndex
            };


            await _transactionManager.CommitTransactionAsync();

            return new ActionResponse<ContractQue>
            {
                WasSuccess = true,
                Result = modelo
            };
        }
        catch (Exception ex)
        {
            await _transactionManager.RollbackTransactionAsync();
            return await _httpErrorHandler.HandleErrorAsync<ContractQue>(ex);
        }
    }

    public async Task<ActionResponse<bool>> DeleteAsync(Guid id)
    {
        await _transactionManager.BeginTransactionAsync();
        try
        {
            var dataRemove = await _context.ContractQues.FindAsync(id);
            if (dataRemove == null)
            {
                return new ActionResponse<bool>
                {
                    WasSuccess = false,
                    Message = _localizer[nameof(Resource.Generic_IdNotFound)]
                };
            }

            _context.ContractQues.Remove(dataRemove);
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
