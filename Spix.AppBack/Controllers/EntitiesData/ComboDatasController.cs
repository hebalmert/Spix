using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using Spix.AppBack.Helper;
using Spix.Domain.EntitiesData;
using Spix.Domain.Enum;
using Spix.DomainLogic.ResponcesSec;
using Spix.UnitOfWork.InterfaceEntities;
using Spix.UnitOfWork.InterfacesEntitiesData;

namespace Spix.AppBack.Controllers.EntitiesData
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/combosData")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Administrator, Auxiliar")]
    [ApiController]
    public class ComboDatasController : ControllerBase
    {
        private readonly ISecurityUnitOfWork _securityUnitOfWork;
        private readonly IOperationUnitOfWork _operationUnitOfWork;
        private readonly IHotSpotTypeUnitOfWork _hotSpotTypeUnitOfWork;
        private readonly IFrecuencyTypeUnitOfWork _frecuencyTypeUnitOfWork;
        private readonly IFrecuencyUnitOfWork _frecuencyUnitOfWork;
        private readonly IChannelUnitOfWork _channelUnitOfWork;
        private readonly IStateUnitOfWork _stateUnitOfWork;
        private readonly ICityUnitOfWork _cityUnitOfWork;
        private readonly IChainTypesUnitOfWork _chainTypesUnitOfWork;
        private readonly IStringLocalizer _localizer;

        public ComboDatasController(ISecurityUnitOfWork securityUnitOfWork, IOperationUnitOfWork operationUnitOfWork,
            IHotSpotTypeUnitOfWork hotSpotTypeUnitOfWork, IFrecuencyTypeUnitOfWork frecuencyTypeUnitOfWork,
            IFrecuencyUnitOfWork frecuencyUnitOfWork, IChannelUnitOfWork channelUnitOfWork, IStateUnitOfWork stateUnitOfWork,
            ICityUnitOfWork cityUnitOfWork,
            IChainTypesUnitOfWork chainTypesUnitOfWork, IStringLocalizer localizer)
        {
            _securityUnitOfWork = securityUnitOfWork;
            _operationUnitOfWork = operationUnitOfWork;
            _hotSpotTypeUnitOfWork = hotSpotTypeUnitOfWork;
            _frecuencyTypeUnitOfWork = frecuencyTypeUnitOfWork;
            _frecuencyUnitOfWork = frecuencyUnitOfWork;
            _channelUnitOfWork = channelUnitOfWork;
            _stateUnitOfWork = stateUnitOfWork;
            _cityUnitOfWork = cityUnitOfWork;
            _chainTypesUnitOfWork = chainTypesUnitOfWork;
            _localizer = localizer;
        }

        [HttpGet("ComboCity/{id}")]
        public async Task<IActionResult> GetComboCity(int id)
        {
            try
            {
                //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
                //ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer);
                var response = await _cityUnitOfWork.ComboAsync(id);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ComboState")]
        public async Task<IActionResult> GetComboState(int id)
        {
            try
            {
                //lo usamos para tomar el Email del Claims, pero Verifica que este Authenticated=true.
                ClaimsDTOs userClaimsInfo = User.GetEmailOrThrow(_localizer, HttpContext);
                var response = await _stateUnitOfWork.ComboAsync(userClaimsInfo);
                return ResponseHelper.Format(response);
            }
            catch (ApplicationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("ComboSecurity")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboSecurityAsync()
        {
            var response = await _securityUnitOfWork.ComboAsync();
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("ComboOperation")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboOperationAsync()
        {
            var response = await _operationUnitOfWork.ComboAsync();
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("ComboHotspot")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboHotspotAsync()
        {
            var response = await _hotSpotTypeUnitOfWork.ComboAsync();
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("ComboFrecuencyType")]
        public async Task<ActionResult<IEnumerable<FrecuencyType>>> GetComboFrecuencyTypeAsync()
        {
            var response = await _frecuencyTypeUnitOfWork.ComboAsync();
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("ComboFrecuency/{id:int}")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboFrecuencyAsync(int id)
        {
            var response = await _frecuencyUnitOfWork.ComboAsync(id);
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("ComboChannel")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboChannelAsync()
        {
            var response = await _channelUnitOfWork.ComboAsync();
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }

        [HttpGet("ComboChainType")]
        public async Task<ActionResult<IEnumerable<IntItemModel>>> GetComboChainTypeAsync()
        {
            var response = await _chainTypesUnitOfWork.ComboAsync();
            if (!response.WasSuccess)
            {
                return BadRequest(response.Message);
            }
            return Ok(response.Result);
        }
    }
}