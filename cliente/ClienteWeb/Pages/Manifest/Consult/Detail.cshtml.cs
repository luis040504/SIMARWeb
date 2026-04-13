using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Consult;

public class DetailModel : PageModel
{
    [BindProperty]
    public RegisterViewModel RegisterInput { get; set; } = new();

    public ManifestDetailViewModel Manifest { get; private set; } = new();

    public IActionResult OnGet(string id)
    {
        var found = SampleData.FirstOrDefault(m => m.Id == id);
        if (found is null) return NotFound();

        Manifest = found;
        return Page();
    }

    public IActionResult OnPostRegister()
    {
        var found = SampleData.FirstOrDefault(m => m.Id == RegisterInput.Id);
        if (found is null) return NotFound();

        if (RegisterInput.SignedFile is null)
            ModelState.AddModelError(nameof(RegisterInput.SignedFile), "Debes subir el PDF firmado.");

        if (!ModelState.IsValid)
        {
            Manifest = found;
            return Page();
        }

        found.Status = ManifestStatus.Completado;
        found.SignedDate = RegisterInput.SignedDate;
        found.SignedManifestFileName = RegisterInput.SignedFile!.FileName;

        TempData["SuccessMessage"] =
            $"El manifiesto {found.ManifestNumber} ha sido marcado como completado.";

        return RedirectToPage(new { id = found.Id });
    }

    public IActionResult OnPostTransitar(string id)
    {
        var found = SampleData.FirstOrDefault(m => m.Id == id);
        if (found is null) return NotFound();

        if (found.Status != ManifestStatus.Borrador)
            return RedirectToPage(new { id });

        found.Status = ManifestStatus.EnTransito;

        TempData["SuccessMessage"] =
            $"El manifiesto {found.ManifestNumber} fue enviado a tránsito.";

        return RedirectToPage(new { id });
    }

    public static List<ManifestDetailViewModel> SampleData { get; } = BuildSampleData();

    private static List<ManifestDetailViewModel> BuildSampleData() =>
    [
        new ManifestDetailViewModel
        {
            Id = "1",
            Type = "especial",
            Status = ManifestStatus.Completado,
            SignedManifestFileName = "009-2026_firmado.pdf",
            SignedDate = new DateOnly(2026, 2, 27),
            ManifestNumber = "009/2026",
            ManifestDate = new DateOnly(2026, 2, 26),
            ManifestTime = new TimeOnly(10, 48),
            EnvironmentalRegistrationNumber = "SEDEMA/TRME-CH0990/20EXR-17/182",

            SocialReason = "Cementos Moctezuma S.A. de C.V.",
            Address = "Dom. conocido Predio de Los Gallineros, Camino Vecinal Cerro Colorado",
            PostalCode = "91645",
            Municipality = "Apazapan",
            PhoneNumber = "2288326510",
            Email = "operaciones@moctezuma.com.mx",
            GeneratorResponsibleName = "Santiago Montoya",
            GeneratorObservations = "",

            SpecialResidues =
            [
                new SpecialResidueItem
                {
                    ResidueKey = "IE-001",
                    ResidueName = "Otros Residuos Inorgánicos (RSU)",
                    ContainerType = "OF",
                    ContainerCapacity = "1/6 m³",
                    Weight = 680,
                    Unit = "kg"
                }
            ],

            TransporterAuthorizationNumber = "SEDEMA/TRME-SMA010239-2025/073",
            TransporterSocialReason = "Sistemas en Manejo y Administración de Residuos S.A. de C.V.",
            TransporterAddress = "Av. Pípila No. 126, Col. José Cardel",
            TransporterPostalCode = "91030",
            TransporterMunicipality = "Xalapa",
            TransporterPhone = "2288343149",
            VehicleType = "Camión Caja Seca 3.5 Ton.",
            VehiclePlate = "YJ-9638-A",
            DriverLicense = "UB0030UNC",
            TransportRoute = "De Apazapan, Ver. a Xalapa, Ver.",
            TransporterResponsibleName = "Fernando López Sánchez",
            TransporterDate = new DateOnly(2026, 2, 26),
            TransporterTime = new TimeOnly(12, 15),
            TransporterObservations = "",

            ReceiverAuthorizationNumber = "SEDEMA/AATRME-SMA0810239LG-2024/16",
            ReceiverSocialReason = "Sistemas en Manejo y Administración de Residuos S.A. de C.V.",
            ReceiverAddress = "Félix Licona #209 Col. Rafael Lucio",
            ReceiverPostalCode = "91110",
            ReceiverMunicipality = "Xalapa",
            ReceiverPhone = "2288190044",
            DisposalType = "Almacén Temporal",
            ReceiverDate = new DateOnly(2026, 2, 26),
            ReceiverResponsibleName = "Gustavo Cruz Torres",
            ReceiverObservations = ""
        },

        new ManifestDetailViewModel
        {
            Id = "2",
            Type = "peligroso",
            Status = ManifestStatus.EnTransito,
            ManifestNumber = "002/2026",
            EnvironmentalRegistrationNumber = "CMORE3001711",

            SocialReason = "Cementos Moctezuma S.A. de C.V.",
            PostalCode = "91645",
            Street = "Dom. conocido Predio de Los Gallineros",
            ExteriorNumber = "S/N",
            InteriorNumber = "S/N",
            Colony = "Camino Vecinal Cerro Colorado",
            Municipality = "Apazapan",
            State = "Veracruz",
            PhoneNumber = "2288326510",
            Email = "operaciones@moctezuma.com.mx",
            SafeHandlingInstructions = "Uso de EPP: guantes, ropa de algodón, calzado de seguridad.",
            GeneratorResponsibleName = "Santiago Montoya",
            GeneratorSignDate = new DateOnly(2026, 2, 26),

            HazardousResidues =
            [
                new HazardousResidueItem
                {
                    ResidueName = "Objetos Punzocortantes",
                    IsBiological = true,
                    ContainerType = "CIP",
                    ContainerCapacity = "1",
                    AmountKg = 250,
                    HasLabel = true
                },
                new HazardousResidueItem
                {
                    ResidueName = "Residuos No Anatómicos",
                    IsBiological = true,
                    ContainerType = "RIP",
                    ContainerCapacity = "N/A",
                    AmountKg = 640,
                    HasLabel = true
                }
            ],

            TransporterAuthorizationNumber = "30-087-PS-I-07D-17",
            TransporterSCTPermit = "3063SMA1005201723031000",
            TransporterSocialReason = "Sistemas en Manejo y Administración de Residuos S.A. de C.V.",
            TransporterStreet = "Avenida Pípila",
            TransporterExteriorNumber = "126",
            TransporterInteriorNumber = "S/N",
            TransporterColony = "José Cardel",
            TransporterPostalCode = "91030",
            TransporterMunicipality = "Xalapa",
            TransporterState = "Veracruz",
            TransporterPhone = "2288343149",
            TransporterEmail = "operaciones@gruposimar.com",
            VehicleType = "Camión Caja Seca 4.5 Ton.",
            VehiclePlate = "39AE4N",
            TransportRoute = "Apazapan, Ver. a Xalapa, Ver.",
            TransporterResponsibleName = "Fernando López Sánchez",
            TransporterSignDate = new DateOnly(2026, 2, 26),

            ReceiverAuthorizationNumber = "21-V-23-20",
            ReceiverSocialReason = "Exitum Tratamientos Ecológicos S.A. de C.V.",
            ReceiverStreet = "Carr. Gpe. Victoria Valsequillo KM15",
            ReceiverExteriorNumber = "147",
            ReceiverInteriorNumber = "Z-3",
            ReceiverColony = "Localidad San Baltasar Torija",
            ReceiverPostalCode = "75235",
            ReceiverMunicipality = "Cuautinchan",
            ReceiverState = "Puebla",
            ReceiverPhone = "(222)239 2323",
            ReceiverEmail = "captura@exitium.mx",
            ReceiverPersonName = "Gustavo Cruz Torres",
            ReceiverResponsibleName = "Responsable Exitum",
            ReceiverObservations = "",
            ReceiverSignDate = new DateOnly(2026, 2, 27)
        },

        new ManifestDetailViewModel
        {
            Id = "3",
            Type = "especial",
            Status = ManifestStatus.EnTransito,
            ManifestNumber = "015/2026",
            ManifestDate = new DateOnly(2026, 1, 15),
            ManifestTime = new TimeOnly(9, 30),
            EnvironmentalRegistrationNumber = "SEDEMA/TRME-VER-XXX",
            SocialReason = "Industrias Veracruz S.A. de C.V.",
            Address = "Calle Industrial 100",
            PostalCode = "91000",
            Municipality = "Xalapa",
            PhoneNumber = "2281111111",
            GeneratorResponsibleName = "Luis Pérez",
            SpecialResidues =
            [
                new SpecialResidueItem
                {
                    ResidueKey = "IE-003",
                    ResidueName = "Residuos de construcción",
                    ContainerType = "Volquete",
                    ContainerCapacity = "1",
                    Weight = 1200,
                    Unit = "kg"
                }
            ],
            TransporterSocialReason = "SIMAR",
            TransporterPhone = "2288343149",
            VehiclePlate = "VER-001",
            DriverLicense = "LIC-001",
            TransportRoute = "Xalapa",
            ReceiverSocialReason = "SIMAR – Almacén Xalapa",
            DisposalType = "Almacén Temporal",
            ReceiverResponsibleName = "Ana Torres"
        },

        new ManifestDetailViewModel
        {
            Id = "4",
            Type = "peligroso",
            Status = ManifestStatus.Borrador,
            ManifestNumber = "005/2026",
            EnvironmentalRegistrationNumber = "CMORE9999999",
            SocialReason = "Hospital Regional IMSS",
            Street = "Av. de la Salud",
            ExteriorNumber = "45",
            InteriorNumber = "S/N",
            Colony = "Centro",
            Municipality = "Veracruz",
            State = "Veracruz",
            PhoneNumber = "2299999999",
            Email = "rpbi@hospital.mx",
            SafeHandlingInstructions = "Uso de EPP completo. Manejo de residuos RPBI.",
            GeneratorResponsibleName = "Dr. Ramírez",
            GeneratorSignDate = new DateOnly(2026, 3, 1),
            HazardousResidues =
            [
                new HazardousResidueItem
                {
                    ResidueName = "Sangre y fluidos corporales",
                    IsBiological = true,
                    ContainerType = "Bolsa roja",
                    ContainerCapacity = "1",
                    AmountKg = 120,
                    HasLabel = true
                }
            ],
            TransporterSocialReason = "SIMAR",
            TransporterPhone = "2288343149",
            VehiclePlate = "VER-002",
            TransportRoute = "Veracruz – Xalapa",
            ReceiverSocialReason = "Exitum Tratamientos Ecológicos",
            ReceiverPersonName = "Gustavo Cruz Torres"
        }
    ];
}

public enum ManifestStatus
{
    Borrador,
    EnTransito,
    Completado
}

public class ManifestDetailViewModel
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = "especial";
    public ManifestStatus Status { get; set; } = ManifestStatus.Borrador;

    public string? SignedManifestFileName { get; set; }
    public DateOnly? SignedDate { get; set; }

    public string ManifestNumber { get; set; } = string.Empty;

    // GENERADOR
    public DateOnly? ManifestDate { get; set; }
    public TimeOnly? ManifestTime { get; set; }
    public string EnvironmentalRegistrationNumber { get; set; } = string.Empty;
    public string SocialReason { get; set; } = string.Empty;

    // Especial
    public string Address { get; set; } = string.Empty;

    // Peligroso
    public string Street { get; set; } = string.Empty;
    public string ExteriorNumber { get; set; } = string.Empty;
    public string InteriorNumber { get; set; } = string.Empty;
    public string Colony { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;
    public string Municipality { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public DateOnly? GeneratorSignDate { get; set; }
    public string GeneratorResponsibleName { get; set; } = string.Empty;
    public string GeneratorObservations { get; set; } = string.Empty;
    public string SafeHandlingInstructions { get; set; } = string.Empty;

    public List<SpecialResidueItem> SpecialResidues { get; set; } = [];
    public List<HazardousResidueItem> HazardousResidues { get; set; } = [];

    // TRANSPORTISTA
    public string TransporterAuthorizationNumber { get; set; } = string.Empty;
    public string TransporterSCTPermit { get; set; } = string.Empty;
    public string TransporterSocialReason { get; set; } = string.Empty;

    // Especial
    public string TransporterAddress { get; set; } = string.Empty;

    // Peligroso
    public string TransporterStreet { get; set; } = string.Empty;
    public string TransporterExteriorNumber { get; set; } = string.Empty;
    public string TransporterInteriorNumber { get; set; } = string.Empty;
    public string TransporterColony { get; set; } = string.Empty;
    public string TransporterState { get; set; } = string.Empty;
    public string TransporterEmail { get; set; } = string.Empty;

    public string TransporterPostalCode { get; set; } = string.Empty;
    public string TransporterMunicipality { get; set; } = string.Empty;
    public string TransporterPhone { get; set; } = string.Empty;
    public string VehicleType { get; set; } = string.Empty;
    public string VehiclePlate { get; set; } = string.Empty;
    public string DriverLicense { get; set; } = string.Empty;
    public string TransportRoute { get; set; } = string.Empty;
    public string TransporterObservations { get; set; } = string.Empty;
    public string TransporterResponsibleName { get; set; } = string.Empty;

    // Especial
    public DateOnly? TransporterDate { get; set; }
    public TimeOnly? TransporterTime { get; set; }

    // Peligroso
    public DateOnly? TransporterSignDate { get; set; }

    // DESTINATARIO
    public string ReceiverAuthorizationNumber { get; set; } = string.Empty;
    public string ReceiverSocialReason { get; set; } = string.Empty;

    // Especial
    public string ReceiverAddress { get; set; } = string.Empty;
    public string DisposalType { get; set; } = string.Empty;
    public DateOnly? ReceiverDate { get; set; }

    // Peligroso
    public string ReceiverStreet { get; set; } = string.Empty;
    public string ReceiverExteriorNumber { get; set; } = string.Empty;
    public string ReceiverInteriorNumber { get; set; } = string.Empty;
    public string ReceiverColony { get; set; } = string.Empty;
    public string ReceiverState { get; set; } = string.Empty;
    public string ReceiverEmail { get; set; } = string.Empty;
    public string ReceiverPersonName { get; set; } = string.Empty;
    public DateOnly? ReceiverSignDate { get; set; }

    public string ReceiverPostalCode { get; set; } = string.Empty;
    public string ReceiverMunicipality { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string ReceiverObservations { get; set; } = string.Empty;
    public string ReceiverResponsibleName { get; set; } = string.Empty;
}

public class SpecialResidueItem
{
    public string ResidueKey { get; set; } = string.Empty;
    public string ResidueName { get; set; } = string.Empty;
    public string ContainerType { get; set; } = string.Empty;
    public string ContainerCapacity { get; set; } = string.Empty;
    public decimal Weight { get; set; }
    public string Unit { get; set; } = "kg";
}

public class HazardousResidueItem
{
    public string ResidueName { get; set; } = string.Empty;
    public bool IsCorrosive { get; set; }
    public bool IsReactive { get; set; }
    public bool IsExplosive { get; set; }
    public bool IsToxic { get; set; }
    public bool IsFlammable { get; set; }
    public bool IsBiological { get; set; }
    public bool IsMutagenic { get; set; }
    public string ContainerType { get; set; } = string.Empty;
    public string ContainerCapacity { get; set; } = string.Empty;
    public decimal AmountKg { get; set; }
    public bool? HasLabel { get; set; }
}