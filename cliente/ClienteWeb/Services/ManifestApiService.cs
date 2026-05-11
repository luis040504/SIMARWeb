using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using ClienteWeb.Pages.Manifest.Consult;
using ClienteWeb.Pages.Manifest.Generate;
using Microsoft.AspNetCore.Http;

namespace ClienteWeb.Services;

public class ManifestApiService
{
    private readonly HttpClient _http;
    private readonly int _defaultClienteId;

    public ManifestApiService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _defaultClienteId = 0; // se sobreescribe con el id del cliente seleccionado en el flujo
    }


    public async Task<List<ManifestSummary>> GetAllAsync(
        string? numero = null, string? razonSocial = null, string? tipo = null,
        string? estado = null, DateOnly? fechaDesde = null, DateOnly? fechaHasta = null,
        int? clienteId = null, int? contratoId = null)
    {
        var qs = new List<string>();
        if (!string.IsNullOrEmpty(numero))       qs.Add($"numero={Uri.EscapeDataString(numero)}");
        if (!string.IsNullOrEmpty(razonSocial))  qs.Add($"razon_social={Uri.EscapeDataString(razonSocial)}");
        if (!string.IsNullOrEmpty(tipo))         qs.Add($"tipo={Uri.EscapeDataString(tipo)}");
        if (!string.IsNullOrEmpty(estado))       qs.Add($"estado={Uri.EscapeDataString(estado)}");
        if (fechaDesde.HasValue)                 qs.Add($"fecha_desde={fechaDesde.Value:yyyy-MM-dd}");
        if (fechaHasta.HasValue)                 qs.Add($"fecha_hasta={fechaHasta.Value:yyyy-MM-dd}");
        if (clienteId.HasValue)                  qs.Add($"id_cliente={clienteId.Value}");
        if (contratoId.HasValue)                 qs.Add($"contrato_id={contratoId.Value}");

        var url = "manifiestos" + (qs.Count > 0 ? "?" + string.Join("&", qs) : "");
        var resp = await _http.GetFromJsonAsync<ApiResponse<List<ManifestListItemDto>>>(url);
        return resp?.Data?.Select(MapToSummary).ToList() ?? [];
    }


    public async Task<ManifestDetailViewModel?> GetByIdAsync(string id)
    {
        var resp = await _http.GetFromJsonAsync<ApiResponse<ManifestDetailApiDto>>($"manifiestos/{id}");
        return resp?.Data is null ? null : MapToDetailViewModel(resp.Data);
    }

    public async Task<string> CreateSpecialAsync(SpecialWasteModel model)
    {
        var dto = new CreateManifestDto
        {
            IdCliente   = model.IdCliente,
            ContratoId  = model.ContratoId > 0 ? model.ContratoId : null,
            Tipo = "especial",
            NumeroRegistroAmbiental = model.EnvironmentalRegistrationNumber,
            RazonSocial = model.SocialReason,
            Domicilio = model.Address,
            CodigoPostal = model.PostalCode,
            Municipio = model.Municipality,
            Telefono = model.PhoneNumber,
            Correo = model.Email,
            FechaManifiesto = model.ManifestDate.ToString("yyyy-MM-dd"),
            HoraManifiesto = model.ManifestTime.ToString("HH:mm:ss"),
            ObservacionesGenerador = model.GeneratorObservations,
            NombreResponsableGenerador = model.GeneratorResponsibleName,
            NumeroAutorizacionTransportista = model.TransporterAuthorizationNumber,
            RazonSocialTransportista = model.TransporterSocialReason,
            DomicilioTransportista = model.TransporterAddress,
            CodigoPostalTransportista = model.TransporterPostalCode,
            MunicipioTransportista = model.TransporterMunicipality,
            TelefonoTransportista = model.TransporterPhone,
            FechaRecepcionTransportista = model.TransporterDate?.ToString("yyyy-MM-dd"),
            HoraRecepcionTransportista = model.TransporterTime?.ToString("HH:mm:ss"),
            TipoVehiculo = model.VehicleType,
            Placa = model.VehiclePlate,
            LicenciaConductor = model.DriverLicense,
            RutaTransporte = model.TransportRoute,
            ObservacionesTransportista = model.TransporterObservations,
            NombreResponsableTransportista = model.TransporterResponsibleName,
            NumeroAutorizacionDestinatario = model.ReceiverAuthorizationNumber,
            RazonSocialDestinatario = model.ReceiverSocialReason,
            DomicilioDestinatario = model.ReceiverAddress,
            CodigoPostalDestinatario = model.ReceiverPostalCode,
            MunicipioDestinatario = model.ReceiverMunicipality,
            TelefonoDestinatario = model.ReceiverPhone,
            FechaDestinatario = model.ReceiverDate?.ToString("yyyy-MM-dd"),
            TipoDisposicion = model.DisposalType,
            ObservacionesDestinatario = model.ReceiverObservations,
            NombreResponsableDestinatario = model.ReceiverResponsibleName,
            ResiduosEspeciales = model.Residues.Select(r => new EspecialResidueSendDto
            {
                ClaveResiduo = r.ResidueKey,
                NombreResiduo = r.ResidueName,
                TipoEnvase = r.ContainerType,
                Capacidad = r.ContainerCapacity,
                Peso = r.Weight,
                Unidad = r.Unit
            }).ToList()
        };

        var response = await _http.PostAsJsonAsync("manifiestos", dto);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ManifestDetailApiDto>>();
        return result?.Data?.NumeroManifiesto ?? "?";
    }

    // ─── CREATE PELIGROSO ─────────────────────────────────────────────────────

    public async Task<string> CreateHazardousAsync(HazardousWasteModel model)
    {
        var dto = new CreateManifestDto
        {
            IdCliente   = model.IdCliente,
            ContratoId  = model.ContratoId > 0 ? model.ContratoId : null,
            Tipo = "peligroso",
            NumeroRegistroAmbiental = model.EnvironmentalRegistrationNumber,
            RazonSocial = model.SocialReason,
            CodigoPostal = model.PostalCode,
            Calle = model.Street,
            NumeroExterior = model.ExteriorNumber,
            NumeroInterior = model.InteriorNumber,
            Colonia = model.Colony,
            Municipio = model.Municipality,
            EstadoGenerador = model.State,
            Telefono = model.PhoneNumber,
            Correo = model.Email,
            InstruccionesManejoSeguro = model.SafeHandlingInstructions,
            NombreResponsableGenerador = model.GeneratorResponsibleName,
            FechaFirmaGenerador = model.GeneratorSignDate?.ToString("yyyy-MM-dd"),
            NumeroAutorizacionTransportista = model.TransporterAuthorizationNumber,
            NumeroPermisoSct = model.TransporterSCTPermit,
            RazonSocialTransportista = model.TransporterSocialReason,
            CodigoPostalTransportista = model.TransporterPostalCode,
            CalleTransportista = model.TransporterStreet,
            NumeroExteriorTransportista = model.TransporterExteriorNumber,
            NumeroInteriorTransportista = model.TransporterInteriorNumber,
            ColoniaTransportista = model.TransporterColony,
            MunicipioTransportista = model.TransporterMunicipality,
            EstadoTransportista = model.TransporterState,
            TelefonoTransportista = model.TransporterPhone,
            CorreoTransportista = model.TransporterEmail,
            TipoVehiculo = model.VehicleType,
            Placa = model.VehiclePlate,
            LicenciaConductor = model.DriverLicense,
            RutaTransporte = model.TransportRoute,
            NombreResponsableTransportista = string.IsNullOrEmpty(model.TransporterResponsibleName) ? model.DriverName : model.TransporterResponsibleName,
            FechaFirmaTransportista = model.TransporterSignDate?.ToString("yyyy-MM-dd"),
            NumeroAutorizacionDestinatario = model.ReceiverAuthorizationNumber,
            RazonSocialDestinatario = model.ReceiverSocialReason,
            CodigoPostalDestinatario = model.ReceiverPostalCode,
            CalleDestinatario = model.ReceiverStreet,
            NumeroExteriorDestinatario = model.ReceiverExteriorNumber,
            NumeroInteriorDestinatario = model.ReceiverInteriorNumber,
            ColoniaDestinatario = model.ReceiverColony,
            MunicipioDestinatario = model.ReceiverMunicipality,
            EstadoDestinatario = model.ReceiverState,
            TelefonoDestinatario = model.ReceiverPhone,
            CorreoDestinatario = model.ReceiverEmail,
            PersonaRecibe = model.ReceiverPersonName,
            ObservacionesDestinatario = model.ReceiverObservations,
            NombreResponsableDestinatario = model.ReceiverResponsibleName,
            FechaFirmaDestinatario = model.ReceiverSignDate?.ToString("yyyy-MM-dd"),
            ResiduosPeligrosos = model.Residues.Select(r => new PeligrosoResidueSendDto
            {
                NombreResiduo = r.ResidueName,
                EsCorrosivo = r.IsCorrosive,
                EsReactivo = r.IsReactive,
                EsExplosivo = r.IsExplosive,
                EsToxico = r.IsToxic,
                EsInflamable = r.IsFlammable,
                EsBiologico = r.IsBiological,
                EsMutagenico = r.IsMutagenic,
                TipoEnvase = r.ContainerType,
                CapacidadEnvase = r.ContainerCapacity,
                CantidadKg = r.AmountKg,
                TieneEtiqueta = r.HasLabel
            }).ToList()
        };

        var response = await _http.PostAsJsonAsync("manifiestos", dto);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<ManifestDetailApiDto>>();
        return result?.Data?.NumeroManifiesto ?? "?";
    }

    // ─── UPDATE (PUT) ─────────────────────────────────────────────────────────

    public async Task UpdateAsync(string id, ManifestDetailViewModel vm)
    {
        var dto = new CreateManifestDto
        {
            IdCliente                      = vm.IdCliente,
            ContratoId                     = vm.ContratoId,
            NumeroManifiesto               = vm.ManifestNumber,
            Tipo                           = vm.Type,
            NumeroRegistroAmbiental        = vm.EnvironmentalRegistrationNumber,
            RazonSocial                    = vm.SocialReason,
            Domicilio                      = vm.Address,
            Calle                          = vm.Street,
            NumeroExterior                 = vm.ExteriorNumber,
            NumeroInterior                 = vm.InteriorNumber,
            Colonia                        = vm.Colony,
            EstadoGenerador                = vm.State,
            CodigoPostal                   = vm.PostalCode,
            Municipio                      = vm.Municipality,
            Telefono                       = vm.PhoneNumber,
            Correo                         = vm.Email,
            FechaManifiesto                = vm.ManifestDate?.ToString("yyyy-MM-dd"),
            HoraManifiesto                 = vm.ManifestTime?.ToString("HH:mm:ss"),
            ObservacionesGenerador         = vm.GeneratorObservations,
            InstruccionesManejoSeguro      = vm.SafeHandlingInstructions,
            NombreResponsableGenerador     = vm.GeneratorResponsibleName,
            FechaFirmaGenerador            = vm.GeneratorSignDate?.ToString("yyyy-MM-dd"),
            NumeroAutorizacionTransportista  = vm.TransporterAuthorizationNumber,
            NumeroPermisoSct                 = vm.TransporterSCTPermit,
            RazonSocialTransportista         = vm.TransporterSocialReason,
            DomicilioTransportista           = vm.TransporterAddress,
            CalleTransportista               = vm.TransporterStreet,
            NumeroExteriorTransportista      = vm.TransporterExteriorNumber,
            NumeroInteriorTransportista      = vm.TransporterInteriorNumber,
            ColoniaTransportista             = vm.TransporterColony,
            EstadoTransportista              = vm.TransporterState,
            CorreoTransportista              = vm.TransporterEmail,
            CodigoPostalTransportista        = vm.TransporterPostalCode,
            MunicipioTransportista           = vm.TransporterMunicipality,
            TelefonoTransportista            = vm.TransporterPhone,
            TipoVehiculo                     = vm.VehicleType,
            Placa                            = vm.VehiclePlate,
            LicenciaConductor                = vm.DriverLicense,
            RutaTransporte                   = vm.TransportRoute,
            ObservacionesTransportista       = vm.TransporterObservations,
            NombreResponsableTransportista   = vm.TransporterResponsibleName,
            FechaRecepcionTransportista      = vm.TransporterDate?.ToString("yyyy-MM-dd"),
            HoraRecepcionTransportista       = vm.TransporterTime?.ToString("HH:mm:ss"),
            FechaFirmaTransportista          = vm.TransporterSignDate?.ToString("yyyy-MM-dd"),
            NumeroAutorizacionDestinatario   = vm.ReceiverAuthorizationNumber,
            RazonSocialDestinatario          = vm.ReceiverSocialReason,
            DomicilioDestinatario            = vm.ReceiverAddress,
            CalleDestinatario                = vm.ReceiverStreet,
            NumeroExteriorDestinatario       = vm.ReceiverExteriorNumber,
            NumeroInteriorDestinatario       = vm.ReceiverInteriorNumber,
            ColoniaDestinatario              = vm.ReceiverColony,
            EstadoDestinatario               = vm.ReceiverState,
            CorreoDestinatario               = vm.ReceiverEmail,
            CodigoPostalDestinatario         = vm.ReceiverPostalCode,
            MunicipioDestinatario            = vm.ReceiverMunicipality,
            TelefonoDestinatario             = vm.ReceiverPhone,
            TipoDisposicion                  = vm.DisposalType,
            FechaDestinatario                = vm.ReceiverDate?.ToString("yyyy-MM-dd"),
            PersonaRecibe                    = vm.ReceiverPersonName,
            FechaFirmaDestinatario           = vm.ReceiverSignDate?.ToString("yyyy-MM-dd"),
            NombreResponsableDestinatario    = vm.ReceiverResponsibleName,
            ObservacionesDestinatario        = vm.ReceiverObservations,
        };

        if (vm.Type == "especial" && vm.SpecialResidues.Count > 0)
            dto.ResiduosEspeciales = vm.SpecialResidues.Select(r => new EspecialResidueSendDto
            {
                ClaveResiduo = r.ResidueKey,
                NombreResiduo = r.ResidueName,
                TipoEnvase = r.ContainerType,
                Capacidad = r.ContainerCapacity,
                Peso = r.Weight,
                Unidad = r.Unit
            }).ToList();
        else if (vm.Type == "peligroso" && vm.HazardousResidues.Count > 0)
            dto.ResiduosPeligrosos = vm.HazardousResidues.Select(r => new PeligrosoResidueSendDto
            {
                NombreResiduo  = r.ResidueName,
                EsCorrosivo    = r.IsCorrosive,
                EsReactivo     = r.IsReactive,
                EsExplosivo    = r.IsExplosive,
                EsToxico       = r.IsToxic,
                EsInflamable   = r.IsFlammable,
                EsBiologico    = r.IsBiological,
                EsMutagenico   = r.IsMutagenic,
                TipoEnvase     = r.ContainerType,
                CapacidadEnvase = r.ContainerCapacity,
                CantidadKg     = r.AmountKg,
                TieneEtiqueta  = r.HasLabel
            }).ToList();

        var response = await _http.PutAsJsonAsync($"manifiestos/{id}", dto);
        if (!response.IsSuccessStatusCode)
        {
            var err = await response.Content.ReadFromJsonAsync<ApiResponse<object>>();
            throw new BillingApiException(
                err?.Message ?? "Error al actualizar el manifiesto.",
                (int)response.StatusCode);
        }
    }

    // ─── PATCH ESTADO ─────────────────────────────────────────────────────────

    public async Task UpdateStatusAsync(string id, string estado, DateOnly? fechaFirma = null)
    {
        var body = new UpdateStatusDto
        {
            Estado = estado,
            FechaFirma = fechaFirma?.ToString("yyyy-MM-dd")
        };
        var response = await _http.PatchAsJsonAsync($"manifiestos/{id}/estado", body);
        response.EnsureSuccessStatusCode();
    }

    // ─── UPLOAD FIRMA ─────────────────────────────────────────────────────────

    public async Task UploadFirmaAsync(string id, IFormFile file)
    {
        using var content = new MultipartFormDataContent();
        await using var stream = file.OpenReadStream();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "pdf", file.FileName);
        var response = await _http.PostAsync($"manifiestos/{id}/firma", content);
        response.EnsureSuccessStatusCode();
    }

    // ─── MAPPINGS ─────────────────────────────────────────────────────────────

    private static ManifestSummary MapToSummary(ManifestListItemDto dto)
    {
        DateOnly.TryParse(dto.FechaManifiesto ?? dto.FechaFirmaGenerador, out var date);
        return new ManifestSummary
        {
            Id              = dto.Id.ToString(),
            ManifestNumber  = dto.NumeroManifiesto,
            Type            = dto.Tipo,
            Status          = dto.Estado,
            SocialReason    = dto.RazonSocial ?? "",
            Municipality    = dto.Municipio ?? "",
            ManifestDate    = date,
            ResidueSummary  = dto.ResumenResiduos ?? "",
            TransporterName = dto.RazonSocialTransportista ?? ""
        };
    }

    private ManifestDetailViewModel MapToDetailViewModel(ManifestDetailApiDto dto)
    {
        var vm = new ManifestDetailViewModel
        {
            Id                             = dto.Id.ToString(),
            IdCliente                      = dto.IdCliente,
            ContratoId                     = dto.ContratoId,
            Type                           = dto.Tipo,
            Status                         = dto.Estado switch
            {
                "en_transito" => ManifestStatus.EnTransito,
                "completado"  => ManifestStatus.Completado,
                _             => ManifestStatus.Borrador
            },
            ManifestNumber                 = dto.NumeroManifiesto,
            SignedManifestFileName          = dto.NombreArchivoFirmado,
            SignedDate                     = ParseDate(dto.FechaFirma),
            EnvironmentalRegistrationNumber = dto.NumeroRegistroAmbiental ?? "",
            SocialReason                   = dto.RazonSocial ?? "",
            Address                        = dto.Domicilio ?? "",
            Street                         = dto.Calle ?? "",
            ExteriorNumber                 = dto.NumeroExterior ?? "",
            InteriorNumber                 = dto.NumeroInterior,
            Colony                         = dto.Colonia ?? "",
            State                          = dto.EstadoGenerador ?? "",
            PostalCode                     = dto.CodigoPostal ?? "",
            Municipality                   = dto.Municipio ?? "",
            PhoneNumber                    = dto.Telefono ?? "",
            Email                          = dto.Correo,
            ManifestDate                   = ParseDate(dto.FechaManifiesto),
            ManifestTime                   = ParseTime(dto.HoraManifiesto),
            GeneratorObservations          = dto.ObservacionesGenerador ?? "",
            SafeHandlingInstructions       = dto.InstruccionesManejoSeguro ?? "",
            GeneratorResponsibleName       = dto.NombreResponsableGenerador ?? "",
            GeneratorSignDate              = ParseDate(dto.FechaFirmaGenerador),
            TransporterAuthorizationNumber = dto.NumeroAutorizacionTransportista ?? "",
            TransporterSCTPermit           = dto.NumeroPermisoSct ?? "",
            TransporterSocialReason        = dto.RazonSocialTransportista ?? "",
            TransporterAddress             = dto.DomicilioTransportista ?? "",
            TransporterStreet              = dto.CalleTransportista ?? "",
            TransporterExteriorNumber      = dto.NumeroExteriorTransportista ?? "",
            TransporterInteriorNumber      = dto.NumeroInteriorTransportista,
            TransporterColony              = dto.ColoniaTransportista ?? "",
            TransporterState               = dto.EstadoTransportista ?? "",
            TransporterEmail               = dto.CorreoTransportista,
            TransporterPostalCode          = dto.CodigoPostalTransportista ?? "",
            TransporterMunicipality        = dto.MunicipioTransportista ?? "",
            TransporterPhone               = dto.TelefonoTransportista ?? "",
            VehicleType                    = dto.TipoVehiculo ?? "",
            VehiclePlate                   = dto.Placa ?? "",
            DriverLicense                  = dto.LicenciaConductor ?? "",
            TransportRoute                 = dto.RutaTransporte ?? "",
            TransporterObservations        = dto.ObservacionesTransportista ?? "",
            TransporterResponsibleName     = dto.NombreResponsableTransportista ?? "",
            TransporterDate                = ParseDate(dto.FechaRecepcionTransportista),
            TransporterTime                = ParseTime(dto.HoraRecepcionTransportista),
            TransporterSignDate            = ParseDate(dto.FechaFirmaTransportista),
            ReceiverAuthorizationNumber    = dto.NumeroAutorizacionDestinatario ?? "",
            ReceiverSocialReason           = dto.RazonSocialDestinatario ?? "",
            ReceiverAddress                = dto.DomicilioDestinatario ?? "",
            ReceiverStreet                 = dto.CalleDestinatario ?? "",
            ReceiverExteriorNumber         = dto.NumeroExteriorDestinatario ?? "",
            ReceiverInteriorNumber         = dto.NumeroInteriorDestinatario,
            ReceiverColony                 = dto.ColoniaDestinatario ?? "",
            ReceiverState                  = dto.EstadoDestinatario ?? "",
            ReceiverEmail                  = dto.CorreoDestinatario,
            ReceiverPostalCode             = dto.CodigoPostalDestinatario ?? "",
            ReceiverMunicipality           = dto.MunicipioDestinatario ?? "",
            ReceiverPhone                  = dto.TelefonoDestinatario ?? "",
            DisposalType                   = dto.TipoDisposicion ?? "",
            ReceiverDate                   = ParseDate(dto.FechaDestinatario),
            ReceiverPersonName             = dto.PersonaRecibe ?? "",
            ReceiverResponsibleName        = dto.NombreResponsableDestinatario ?? "",
            ReceiverObservations           = dto.ObservacionesDestinatario ?? "",
            ReceiverSignDate               = ParseDate(dto.FechaFirmaDestinatario),
            FirmaUrl = string.IsNullOrEmpty(dto.NombreArchivoFirmado)
                ? null
                : $"{_http.BaseAddress}manifiestos/{dto.Id}/firma",
        };

        if (dto.Residuos.HasValue && dto.Residuos.Value.ValueKind == JsonValueKind.Array)
        {
            // La API devuelve los decimales como strings ("0.10"), se necesita AllowReadingFromString
            var numOpts = new JsonSerializerOptions
            {
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
                PropertyNameCaseInsensitive = true
            };

            if (dto.Tipo == "especial")
            {
                var list = dto.Residuos.Value.Deserialize<List<EspecialResidueApiDto>>(numOpts);
                if (list is not null)
                    vm.SpecialResidues = list.Select(r => new SpecialResidueItem
                    {
                        ResidueKey        = r.ClaveResiduo ?? "",
                        ResidueName       = r.NombreResiduo ?? "",
                        ContainerType     = r.TipoEnvase ?? "",
                        ContainerCapacity = r.Capacidad ?? "",
                        Weight            = r.Peso,
                        Unit              = r.Unidad ?? "kg"
                    }).ToList();
            }
            else
            {
                var list = dto.Residuos.Value.Deserialize<List<PeligrosoResidueApiDto>>(numOpts);
                if (list is not null)
                    vm.HazardousResidues = list.Select(r => new HazardousResidueItem
                    {
                        ResidueName       = r.NombreResiduo ?? "",
                        IsCorrosive       = r.EsCorrosivo,
                        IsReactive        = r.EsReactivo,
                        IsExplosive       = r.EsExplosivo,
                        IsToxic           = r.EsToxico,
                        IsFlammable       = r.EsInflamable,
                        IsBiological      = r.EsBiologico,
                        IsMutagenic       = r.EsMutagenico,
                        ContainerType     = r.TipoEnvase ?? "",
                        ContainerCapacity = r.CapacidadEnvase ?? "",
                        AmountKg          = r.CantidadKg,
                        HasLabel          = r.TieneEtiqueta
                    }).ToList();
            }
        }

        return vm;
    }

    private static DateOnly? ParseDate(string? s) =>
        s is not null && DateOnly.TryParse(s, out var d) ? d : null;

    private static TimeOnly? ParseTime(string? s) =>
        s is not null && TimeOnly.TryParse(s, out var t) ? t : null;

    // ─── DTOs INTERNOS ────────────────────────────────────────────────────────

    private class ApiResponse<T>
    {
        [JsonPropertyName("success")] public bool Success { get; set; }
        [JsonPropertyName("data")]    public T? Data { get; set; }
        [JsonPropertyName("message")] public string? Message { get; set; }
    }

    private class ManifestListItemDto
    {
        [JsonPropertyName("id")]                       public int Id { get; set; }
        [JsonPropertyName("numero_manifiesto")]        public string NumeroManifiesto { get; set; } = "";
        [JsonPropertyName("tipo")]                     public string Tipo { get; set; } = "";
        [JsonPropertyName("estado")]                   public string Estado { get; set; } = "";
        [JsonPropertyName("razon_social")]             public string? RazonSocial { get; set; }
        [JsonPropertyName("municipio")]                public string? Municipio { get; set; }
        [JsonPropertyName("fecha_manifiesto")]         public string? FechaManifiesto { get; set; }
        [JsonPropertyName("fecha_firma_generador")]    public string? FechaFirmaGenerador { get; set; }
        [JsonPropertyName("resumen_residuos")]         public string? ResumenResiduos { get; set; }
        [JsonPropertyName("razon_social_transportista")] public string? RazonSocialTransportista { get; set; }
    }

    private class ManifestDetailApiDto
    {
        [JsonPropertyName("id")]                       public int Id { get; set; }
        [JsonPropertyName("id_cliente")]               public int IdCliente { get; set; }
        [JsonPropertyName("contrato_id")]              public int? ContratoId { get; set; }
        [JsonPropertyName("numero_manifiesto")]        public string NumeroManifiesto { get; set; } = "";
        [JsonPropertyName("tipo")]                     public string Tipo { get; set; } = "";
        [JsonPropertyName("estado")]                   public string Estado { get; set; } = "";
        [JsonPropertyName("nombre_archivo_firmado")]   public string? NombreArchivoFirmado { get; set; }
        [JsonPropertyName("fecha_firma")]              public string? FechaFirma { get; set; }
        // Generador
        [JsonPropertyName("numero_registro_ambiental")]  public string? NumeroRegistroAmbiental { get; set; }
        [JsonPropertyName("razon_social")]               public string? RazonSocial { get; set; }
        [JsonPropertyName("domicilio")]                  public string? Domicilio { get; set; }
        [JsonPropertyName("calle")]                      public string? Calle { get; set; }
        [JsonPropertyName("numero_exterior")]            public string? NumeroExterior { get; set; }
        [JsonPropertyName("numero_interior")]            public string? NumeroInterior { get; set; }
        [JsonPropertyName("colonia")]                    public string? Colonia { get; set; }
        [JsonPropertyName("estado_generador")]           public string? EstadoGenerador { get; set; }
        [JsonPropertyName("codigo_postal")]              public string? CodigoPostal { get; set; }
        [JsonPropertyName("municipio")]                  public string? Municipio { get; set; }
        [JsonPropertyName("telefono")]                   public string? Telefono { get; set; }
        [JsonPropertyName("correo")]                     public string? Correo { get; set; }
        [JsonPropertyName("fecha_manifiesto")]           public string? FechaManifiesto { get; set; }
        [JsonPropertyName("hora_manifiesto")]            public string? HoraManifiesto { get; set; }
        [JsonPropertyName("observaciones_generador")]    public string? ObservacionesGenerador { get; set; }
        [JsonPropertyName("instrucciones_manejo_seguro")] public string? InstruccionesManejoSeguro { get; set; }
        [JsonPropertyName("nombre_responsable_generador")] public string? NombreResponsableGenerador { get; set; }
        [JsonPropertyName("fecha_firma_generador")]      public string? FechaFirmaGenerador { get; set; }
        // Transportista
        [JsonPropertyName("numero_autorizacion_transportista")] public string? NumeroAutorizacionTransportista { get; set; }
        [JsonPropertyName("numero_permiso_sct")]                public string? NumeroPermisoSct { get; set; }
        [JsonPropertyName("razon_social_transportista")]        public string? RazonSocialTransportista { get; set; }
        [JsonPropertyName("domicilio_transportista")]           public string? DomicilioTransportista { get; set; }
        [JsonPropertyName("calle_transportista")]               public string? CalleTransportista { get; set; }
        [JsonPropertyName("numero_exterior_transportista")]     public string? NumeroExteriorTransportista { get; set; }
        [JsonPropertyName("numero_interior_transportista")]     public string? NumeroInteriorTransportista { get; set; }
        [JsonPropertyName("colonia_transportista")]             public string? ColoniaTransportista { get; set; }
        [JsonPropertyName("estado_transportista")]              public string? EstadoTransportista { get; set; }
        [JsonPropertyName("correo_transportista")]              public string? CorreoTransportista { get; set; }
        [JsonPropertyName("codigo_postal_transportista")]       public string? CodigoPostalTransportista { get; set; }
        [JsonPropertyName("municipio_transportista")]           public string? MunicipioTransportista { get; set; }
        [JsonPropertyName("telefono_transportista")]            public string? TelefonoTransportista { get; set; }
        [JsonPropertyName("tipo_vehiculo")]                     public string? TipoVehiculo { get; set; }
        [JsonPropertyName("placa")]                             public string? Placa { get; set; }
        [JsonPropertyName("licencia_conductor")]                public string? LicenciaConductor { get; set; }
        [JsonPropertyName("ruta_transporte")]                   public string? RutaTransporte { get; set; }
        [JsonPropertyName("observaciones_transportista")]       public string? ObservacionesTransportista { get; set; }
        [JsonPropertyName("nombre_responsable_transportista")]  public string? NombreResponsableTransportista { get; set; }
        [JsonPropertyName("fecha_recepcion_transportista")]     public string? FechaRecepcionTransportista { get; set; }
        [JsonPropertyName("hora_recepcion_transportista")]      public string? HoraRecepcionTransportista { get; set; }
        [JsonPropertyName("fecha_firma_transportista")]         public string? FechaFirmaTransportista { get; set; }
        // Destinatario
        [JsonPropertyName("numero_autorizacion_destinatario")]  public string? NumeroAutorizacionDestinatario { get; set; }
        [JsonPropertyName("razon_social_destinatario")]         public string? RazonSocialDestinatario { get; set; }
        [JsonPropertyName("domicilio_destinatario")]            public string? DomicilioDestinatario { get; set; }
        [JsonPropertyName("calle_destinatario")]                public string? CalleDestinatario { get; set; }
        [JsonPropertyName("numero_exterior_destinatario")]      public string? NumeroExteriorDestinatario { get; set; }
        [JsonPropertyName("numero_interior_destinatario")]      public string? NumeroInteriorDestinatario { get; set; }
        [JsonPropertyName("colonia_destinatario")]              public string? ColoniaDestinatario { get; set; }
        [JsonPropertyName("estado_destinatario")]               public string? EstadoDestinatario { get; set; }
        [JsonPropertyName("correo_destinatario")]               public string? CorreoDestinatario { get; set; }
        [JsonPropertyName("codigo_postal_destinatario")]        public string? CodigoPostalDestinatario { get; set; }
        [JsonPropertyName("municipio_destinatario")]            public string? MunicipioDestinatario { get; set; }
        [JsonPropertyName("telefono_destinatario")]             public string? TelefonoDestinatario { get; set; }
        [JsonPropertyName("tipo_disposicion")]                  public string? TipoDisposicion { get; set; }
        [JsonPropertyName("fecha_destinatario")]                public string? FechaDestinatario { get; set; }
        [JsonPropertyName("hora_destinatario")]                 public string? HoraDestinatario { get; set; }
        [JsonPropertyName("persona_recibe")]                    public string? PersonaRecibe { get; set; }
        [JsonPropertyName("fecha_firma_destinatario")]          public string? FechaFirmaDestinatario { get; set; }
        [JsonPropertyName("nombre_responsable_destinatario")]   public string? NombreResponsableDestinatario { get; set; }
        [JsonPropertyName("observaciones_destinatario")]        public string? ObservacionesDestinatario { get; set; }
        // Array de residuos (especiales o peligrosos según tipo)
        [JsonPropertyName("residuos")] public JsonElement? Residuos { get; set; }
    }

    private class EspecialResidueApiDto
    {
        [JsonPropertyName("clave_residuo")] public string? ClaveResiduo { get; set; }
        [JsonPropertyName("nombre_residuo")] public string? NombreResiduo { get; set; }
        [JsonPropertyName("tipo_envase")]    public string? TipoEnvase { get; set; }
        [JsonPropertyName("capacidad")]      public string? Capacidad { get; set; }
        [JsonPropertyName("peso")]           public decimal Peso { get; set; }
        [JsonPropertyName("unidad")]         public string? Unidad { get; set; }
    }

    private class PeligrosoResidueApiDto
    {
        [JsonPropertyName("nombre_residuo")]  public string? NombreResiduo { get; set; }
        [JsonPropertyName("es_corrosivo")]    [JsonConverter(typeof(JsonBoolConverter))] public bool EsCorrosivo { get; set; }
        [JsonPropertyName("es_reactivo")]     [JsonConverter(typeof(JsonBoolConverter))] public bool EsReactivo { get; set; }
        [JsonPropertyName("es_explosivo")]    [JsonConverter(typeof(JsonBoolConverter))] public bool EsExplosivo { get; set; }
        [JsonPropertyName("es_toxico")]       [JsonConverter(typeof(JsonBoolConverter))] public bool EsToxico { get; set; }
        [JsonPropertyName("es_inflamable")]   [JsonConverter(typeof(JsonBoolConverter))] public bool EsInflamable { get; set; }
        [JsonPropertyName("es_biologico")]    [JsonConverter(typeof(JsonBoolConverter))] public bool EsBiologico { get; set; }
        [JsonPropertyName("es_mutagenico")]   [JsonConverter(typeof(JsonBoolConverter))] public bool EsMutagenico { get; set; }
        [JsonPropertyName("tipo_envase")]     public string? TipoEnvase { get; set; }
        [JsonPropertyName("capacidad_envase")] public string? CapacidadEnvase { get; set; }
        [JsonPropertyName("cantidad_kg")]     public decimal CantidadKg { get; set; }
        [JsonPropertyName("tiene_etiqueta")]  [JsonConverter(typeof(JsonNullableBoolConverter))] public bool? TieneEtiqueta { get; set; }
    }

    private class CreateManifestDto
    {
        [JsonPropertyName("id_cliente")]  public int? IdCliente { get; set; }
        [JsonPropertyName("contrato_id")] public int? ContratoId { get; set; }
        [JsonPropertyName("numero_manifiesto")] public string? NumeroManifiesto { get; set; }
        [JsonPropertyName("tipo")]        public string Tipo { get; set; } = "";
        // Generador
        [JsonPropertyName("numero_registro_ambiental")]    public string? NumeroRegistroAmbiental { get; set; }
        [JsonPropertyName("razon_social")]                 public string? RazonSocial { get; set; }
        [JsonPropertyName("domicilio")]                    public string? Domicilio { get; set; }
        [JsonPropertyName("calle")]                        public string? Calle { get; set; }
        [JsonPropertyName("numero_exterior")]              public string? NumeroExterior { get; set; }
        [JsonPropertyName("numero_interior")]              public string? NumeroInterior { get; set; }
        [JsonPropertyName("colonia")]                      public string? Colonia { get; set; }
        [JsonPropertyName("estado_generador")]             public string? EstadoGenerador { get; set; }
        [JsonPropertyName("codigo_postal")]                public string? CodigoPostal { get; set; }
        [JsonPropertyName("municipio")]                    public string? Municipio { get; set; }
        [JsonPropertyName("telefono")]                     public string? Telefono { get; set; }
        [JsonPropertyName("correo")]                       public string? Correo { get; set; }
        [JsonPropertyName("fecha_manifiesto")]             public string? FechaManifiesto { get; set; }
        [JsonPropertyName("hora_manifiesto")]              public string? HoraManifiesto { get; set; }
        [JsonPropertyName("observaciones_generador")]      public string? ObservacionesGenerador { get; set; }
        [JsonPropertyName("instrucciones_manejo_seguro")]  public string? InstruccionesManejoSeguro { get; set; }
        [JsonPropertyName("nombre_responsable_generador")] public string? NombreResponsableGenerador { get; set; }
        [JsonPropertyName("fecha_firma_generador")]        public string? FechaFirmaGenerador { get; set; }
        // Transportista
        [JsonPropertyName("numero_autorizacion_transportista")]  public string? NumeroAutorizacionTransportista { get; set; }
        [JsonPropertyName("numero_permiso_sct")]                 public string? NumeroPermisoSct { get; set; }
        [JsonPropertyName("razon_social_transportista")]         public string? RazonSocialTransportista { get; set; }
        [JsonPropertyName("domicilio_transportista")]            public string? DomicilioTransportista { get; set; }
        [JsonPropertyName("calle_transportista")]                public string? CalleTransportista { get; set; }
        [JsonPropertyName("numero_exterior_transportista")]      public string? NumeroExteriorTransportista { get; set; }
        [JsonPropertyName("numero_interior_transportista")]      public string? NumeroInteriorTransportista { get; set; }
        [JsonPropertyName("colonia_transportista")]              public string? ColoniaTransportista { get; set; }
        [JsonPropertyName("estado_transportista")]               public string? EstadoTransportista { get; set; }
        [JsonPropertyName("correo_transportista")]               public string? CorreoTransportista { get; set; }
        [JsonPropertyName("codigo_postal_transportista")]        public string? CodigoPostalTransportista { get; set; }
        [JsonPropertyName("municipio_transportista")]            public string? MunicipioTransportista { get; set; }
        [JsonPropertyName("telefono_transportista")]             public string? TelefonoTransportista { get; set; }
        [JsonPropertyName("tipo_vehiculo")]                      public string? TipoVehiculo { get; set; }
        [JsonPropertyName("placa")]                              public string? Placa { get; set; }
        [JsonPropertyName("licencia_conductor")]                 public string? LicenciaConductor { get; set; }
        [JsonPropertyName("ruta_transporte")]                    public string? RutaTransporte { get; set; }
        [JsonPropertyName("observaciones_transportista")]        public string? ObservacionesTransportista { get; set; }
        [JsonPropertyName("nombre_responsable_transportista")]   public string? NombreResponsableTransportista { get; set; }
        [JsonPropertyName("fecha_recepcion_transportista")]      public string? FechaRecepcionTransportista { get; set; }
        [JsonPropertyName("hora_recepcion_transportista")]       public string? HoraRecepcionTransportista { get; set; }
        [JsonPropertyName("fecha_firma_transportista")]          public string? FechaFirmaTransportista { get; set; }
        // Destinatario
        [JsonPropertyName("numero_autorizacion_destinatario")]  public string? NumeroAutorizacionDestinatario { get; set; }
        [JsonPropertyName("razon_social_destinatario")]         public string? RazonSocialDestinatario { get; set; }
        [JsonPropertyName("domicilio_destinatario")]            public string? DomicilioDestinatario { get; set; }
        [JsonPropertyName("calle_destinatario")]                public string? CalleDestinatario { get; set; }
        [JsonPropertyName("numero_exterior_destinatario")]      public string? NumeroExteriorDestinatario { get; set; }
        [JsonPropertyName("numero_interior_destinatario")]      public string? NumeroInteriorDestinatario { get; set; }
        [JsonPropertyName("colonia_destinatario")]              public string? ColoniaDestinatario { get; set; }
        [JsonPropertyName("estado_destinatario")]               public string? EstadoDestinatario { get; set; }
        [JsonPropertyName("correo_destinatario")]               public string? CorreoDestinatario { get; set; }
        [JsonPropertyName("codigo_postal_destinatario")]        public string? CodigoPostalDestinatario { get; set; }
        [JsonPropertyName("municipio_destinatario")]            public string? MunicipioDestinatario { get; set; }
        [JsonPropertyName("telefono_destinatario")]             public string? TelefonoDestinatario { get; set; }
        [JsonPropertyName("tipo_disposicion")]                  public string? TipoDisposicion { get; set; }
        [JsonPropertyName("fecha_destinatario")]                public string? FechaDestinatario { get; set; }
        [JsonPropertyName("persona_recibe")]                    public string? PersonaRecibe { get; set; }
        [JsonPropertyName("fecha_firma_destinatario")]          public string? FechaFirmaDestinatario { get; set; }
        [JsonPropertyName("nombre_responsable_destinatario")]   public string? NombreResponsableDestinatario { get; set; }
        [JsonPropertyName("observaciones_destinatario")]        public string? ObservacionesDestinatario { get; set; }
        // Residuos
        [JsonPropertyName("residuos_especiales")] public List<EspecialResidueSendDto>? ResiduosEspeciales { get; set; }
        [JsonPropertyName("residuos_peligrosos")] public List<PeligrosoResidueSendDto>? ResiduosPeligrosos { get; set; }
    }

    private class EspecialResidueSendDto
    {
        [JsonPropertyName("clave_residuo")] public string? ClaveResiduo { get; set; }
        [JsonPropertyName("nombre_residuo")] public string? NombreResiduo { get; set; }
        [JsonPropertyName("tipo_envase")]    public string? TipoEnvase { get; set; }
        [JsonPropertyName("capacidad")]      public string? Capacidad { get; set; }
        [JsonPropertyName("peso")]           public decimal Peso { get; set; }
        [JsonPropertyName("unidad")]         public string? Unidad { get; set; }
    }

    private class PeligrosoResidueSendDto
    {
        [JsonPropertyName("nombre_residuo")]   public string? NombreResiduo { get; set; }
        [JsonPropertyName("es_corrosivo")]     public bool EsCorrosivo { get; set; }
        [JsonPropertyName("es_reactivo")]      public bool EsReactivo { get; set; }
        [JsonPropertyName("es_explosivo")]     public bool EsExplosivo { get; set; }
        [JsonPropertyName("es_toxico")]        public bool EsToxico { get; set; }
        [JsonPropertyName("es_inflamable")]    public bool EsInflamable { get; set; }
        [JsonPropertyName("es_biologico")]     public bool EsBiologico { get; set; }
        [JsonPropertyName("es_mutagenico")]    public bool EsMutagenico { get; set; }
        [JsonPropertyName("tipo_envase")]      public string? TipoEnvase { get; set; }
        [JsonPropertyName("capacidad_envase")] public string? CapacidadEnvase { get; set; }
        [JsonPropertyName("cantidad_kg")]      public decimal CantidadKg { get; set; }
        [JsonPropertyName("tiene_etiqueta")]   public bool? TieneEtiqueta { get; set; }
    }

    private class UpdateStatusDto
    {
        [JsonPropertyName("estado")]      public string Estado { get; set; } = "";
        [JsonPropertyName("fecha_firma")] public string? FechaFirma { get; set; }
    }

    public class JsonBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Number) return reader.GetInt32() != 0;
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                return s == "1" || s?.ToLower() == "true";
            }
            return false;
        }
        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options) =>
            writer.WriteBooleanValue(value);
    }

    public class JsonNullableBoolConverter : JsonConverter<bool?>
    {
        public override bool? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.Null) return null;
            if (reader.TokenType == JsonTokenType.Number) return reader.GetInt32() != 0;
            if (reader.TokenType == JsonTokenType.True) return true;
            if (reader.TokenType == JsonTokenType.False) return false;
            if (reader.TokenType == JsonTokenType.String)
            {
                var s = reader.GetString();
                if (string.IsNullOrEmpty(s)) return null;
                return s == "1" || s?.ToLower() == "true";
            }
            return null;
        }
        public override void Write(Utf8JsonWriter writer, bool? value, JsonSerializerOptions options)
        {
            if (value == null) writer.WriteNullValue();
            else writer.WriteBooleanValue(value.Value);
        }
    }
}
