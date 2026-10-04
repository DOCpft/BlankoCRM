using AutoMapper;
using BlankoCRM.Application.Abstract;
using BlankoCRM.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace BlankoCRM.Web.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/public/{organizationSlug}/services")]
public sealed class PublicServicesController : ControllerBase
{
    private readonly IPublicServiceCatalog _serviceCatalog;
    private readonly IMapper _mapper;

    public PublicServicesController(IPublicServiceCatalog serviceCatalog, IMapper mapper)
    {
        _serviceCatalog = serviceCatalog;
        _mapper = mapper;
    }

    [HttpGet]
    [EnableRateLimiting("public-appointment-read")]
    [ProducesResponseType(typeof(IReadOnlyList<ServiceDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ServiceDto>>> Get(CancellationToken cancellationToken)
    {
        var services = await _serviceCatalog.GetServicesAsync(cancellationToken);
        return Ok(_mapper.Map<List<ServiceDto>>(services));
    }
}