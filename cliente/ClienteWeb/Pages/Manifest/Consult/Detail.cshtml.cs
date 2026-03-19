using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ClienteWeb.Pages.Manifest.Consult;

public class DetailModel : PageModel
{
    public ManifestDetailViewModel Manifest { get; private set; } = new();

    public IActionResult OnGet(string id)
    {
        var found = SampleData.FirstOrDefault(m => m.Id == id);
        if (found is null) return NotFound();

        Manifest = found;
        return Page();
    }

    // TODO: Reemplazar con llamada real a la API de SIMAR
    public static List<ManifestDetailViewModel> SampleData { get; } = BuildSampleData();

    private static List<ManifestDetailViewModel> BuildSampleData() =>
    [
        new ManifestDetailViewModel
        {
            Id                              = "1",
            Type                            = "especial",
            Status                          = ManifestStatus.Completado,
            SignedManifestFileName          = "OS-990226_firmado.pdf",
            SignedDate                      = new DateOnly(2026, 2, 27),
            ManifestNumber                  = "OS-990226",
            ManifestDate                    = new DateOnly(2026, 2, 26),
            ManifestTime                    = new TimeOnly(10, 48),
            EnvironmentalRegistrationNumber = "SEDEMA/TRME-CH0990/20EXR-17/182",

            SocialReason                    = "Cementos Moctezuma S.A. de C.V.",
            Address                         = "Dom. conocido Predio de Los Gallineros, Camino Vecinal Cerro Colorado",
            PostalCode                      = "91645",
            Municipality                    = "Apazapan",
            PhoneNumber                     = "2288326510",
            RegulatoryFramework             = "LGPGIR",
            GeneratorResponsibleName        = "Santiago Montoya",
            GeneratorObservations           = "",

            SpecialResidues =
            [
                new SpecialResidueItem
                {
                    ResidueKey        = "IE-001",
                    ResidueName       = "Otros Residuos Inorgánicos (RSU)",
                    Container         = "OF",
                    ContainerCapacity = "1/6 m³",
                    Amount            = 680,
                    Unit              = "kg"
                }
            ],

            TransporterAuthorizationNumber  = "SEDEMA/TRME-SMA010239-2025/073",
            TransporterSocialReason         = "Sistemas en Manejo y Administración de Residuos S.A. de C.V.",
            TransporterAddress              = "Av. Répila Nº 126, Col. José Cardel",
            TransporterPostalCode           = "91030",
            TransporterMunicipality         = "Xalapa",
            TransporterPhone                = "228 834 3149",
            VehicleType                     = "Camión Caja Seca 3.5 Ton.",
            VehiclePlate                    = "YJ-9638-A",
            DriverName                      = "Fernando Lopez Sanchez",
            DriverLicense                   = "UB0030UNC",
            TransportRoute                  = "De Apazapan, Ver. a Xalapa, Ver.",
            TransporterResponsibleName      = "Fernando Lopez Sanchez",

            ReceiverAuthorizationNumber     = "SEDEMA/AATRME-SMA0810239LG-2024/16",
            ReceiverSocialReason            = "Sistemas en Manejo y Administración de Residuos S.A. de C.V.",
            ReceiverAddress                 = "Félix Licona #209 Col. Rafael Lucio",
            ReceiverPostalCode              = "91110",
            ReceiverMunicipality            = "Xalapa",
            ReceiverPhone                   = "2288190044",
            DisposalType                    = "Almacén Temporal",
            ReceiverDate                    = new DateOnly(2026, 2, 26),
            ReceiverResponsibleName         = "Gustavo Cruz Torres"
        },

        new ManifestDetailViewModel
        {
            Id                              = "2",
            Type                            = "peligroso",
            Status                          = ManifestStatus.EnTransito,
            ManifestNumber                  = "002/2026",
            ManifestDate                    = new DateOnly(2026, 2, 26),
            EnvironmentalRegistrationNumber = "CMORE3001711",

            SocialReason                    = "Cementos Moctezuma S.A. de C.V.",
            PostalCode                      = "91645",
            Street                          = "Dom. conocido Predio de Los Gallineros",
            Colony                          = "Camino Vecinal Cerro Colorado",
            Municipality                    = "Apazapan",
            State                           = "Veracruz",
            PhoneNumber                     = "2288326510",
            SafeHandlingInstructions        = "Uso de EPP: Guantes, ropa de algodón, calzado de seguridad",
            RegulatoryFramework             = "LGPGIR / NOM-087",
            GeneratorResponsibleName        = "Santiago Montoya",

            HazardousResidues =
            [
                new HazardousResidueItem
                {
                    ResidueName       = "Objetos Punzocortantes (B)",
                    IsExplosive       = true,
                    Container         = "CIP",
                    ContainerCapacity = "1",
                    AmountKg          = 250,
                    HasLabel          = true
                },
                new HazardousResidueItem
                {
                    ResidueName       = "Residuos No Anatómicos (B)",
                    IsBiological      = true,
                    Container         = "RIP",
                    ContainerCapacity = "N/A",
                    AmountKg          = 640,
                    HasLabel          = true
                }
            ],

            TransporterAuthorizationNumber  = "30-087-PS-I-07D-17",
            TransporterSCTPermit            = "3063SMA1005201723031000",
            TransporterSocialReason         = "Sistemas en Manejo y Administración de Residuos S.A. de C.V.",
            TransporterStreet               = "Avenida Pipila",
            TransporterExteriorNumber       = "126",
            TransporterPostalCode           = "91030",
            TransporterMunicipality         = "Xalapa",
            TransporterState                = "Veracruz",
            TransporterPhone                = "228 834 3149",
            TransporterEmail                = "operaciones@gruposimar.com",
            VehicleType                     = "Camión Caja Seca 4.5 Ton.",
            VehiclePlate                    = "39AE4N",
            TransportRoute                  = "Apazapan, Ver. A",
            DriverName                      = "Fernando Lopez Sanchez",
            TransporterReceptionDate        = new DateOnly(2026, 2, 26),
            TransporterResponsibleName      = "Fernando Lopez Sanchez",

            // Destinatario
            ReceiverAuthorizationNumber     = "21-V-23-20",
            ReceiverSocialReason            = "Exitum Tratamientos Ecológicos S.A. de C.V.",
            ReceiverStreet                  = "Carr. Gpe. Victoria Valsequillo KM15",
            ReceiverExteriorNumber          = "PARCELA",
            ReceiverInteriorNumber          = "2-3",
            ReceiverColony                  = "Localidad San Baltasar Torua",
            ReceiverPostalCode              = "75235",
            ReceiverMunicipality            = "Cuautinchan",
            ReceiverState                   = "Puebla",
            ReceiverPhone                   = "(222)239 2323",
            ReceiverEmail                   = "captura@exitium.mx",
            ReceiverPersonName              = "Gustavo Cruz Torres"
        },

        new ManifestDetailViewModel
        {
            Id                              = "3",
            Type                            = "especial",
            Status                          = ManifestStatus.Impreso,
            ManifestNumber                  = "OS-000543",
            ManifestDate                    = new DateOnly(2026, 1, 15),
            EnvironmentalRegistrationNumber = "SEDEMA/TRME-VER-XXX",
            SocialReason                    = "Industrias Veracruz S.A. de C.V.",
            Address                         = "Calle Industrial 100",
            PostalCode                      = "91000",
            Municipality                    = "Xalapa",
            PhoneNumber                     = "2281111111",
            GeneratorResponsibleName        = "Luis Pérez",
            SpecialResidues =
            [
                new SpecialResidueItem
                {
                    ResidueKey  = "IE-003",
                    ResidueName = "Residuos de construcción",
                    Container   = "volquete",
                    Amount      = 1200,
                    Unit        = "kg"
                }
            ],
            TransporterSocialReason    = "SIMAR",
            TransporterPhone           = "228 834 3149",
            VehiclePlate               = "VER-001",
            DriverName                 = "Carlos Soto",
            TransportRoute             = "Xalapa",
            ReceiverSocialReason       = "SIMAR – Almacén Xalapa",
            DisposalType               = "Almacén Temporal",
            ReceiverResponsibleName    = "Ana Torres"
        },

        // ── Peligroso adicional – Borrador ────────────────────────────────
        new ManifestDetailViewModel
        {
            Id                              = "4",
            Type                            = "peligroso",
            Status                          = ManifestStatus.Borrador,
            ManifestNumber                  = "005/2026",
            ManifestDate                    = new DateOnly(2026, 3, 1),
            EnvironmentalRegistrationNumber = "CMORE9999999",
            SocialReason                    = "Hospital Regional IMSS",
            Street                          = "Av. de la Salud",
            Municipality                    = "Veracruz",
            State                           = "Veracruz",
            PhoneNumber                     = "2299999999",
            SafeHandlingInstructions        = "Uso de EPP completo. Manejo de residuos RPBI.",
            GeneratorResponsibleName        = "Dr. Ramírez",
            HazardousResidues =
            [
                new HazardousResidueItem
                {
                    ResidueName  = "Sangre y fluidos corporales",
                    IsBiological = true,
                    Container    = "bolsa roja",
                    AmountKg     = 120,
                    HasLabel     = true
                }
            ],
            TransporterSocialReason    = "SIMAR",
            TransporterPhone           = "228 834 3149",
            VehiclePlate               = "VER-002",
            DriverName                 = "José Méndez",
            TransportRoute             = "Veracruz – Xalapa",
            ReceiverSocialReason       = "Exitum Tratamientos Ecológicos",
            ReceiverPersonName         = "Gustavo Cruz Torres"
        }
    ];
}

// ── Enumeración de estados ───────────────────────────────────────────────────

public enum ManifestStatus
{
    Borrador,
    Impreso,
    EnTransito,
    Completado
}


public class ManifestDetailViewModel
{
    public string   Id      { get; set; } = string.Empty;
    /// <summary>"especial" | "peligroso"</summary>
    public string   Type    { get; set; } = "especial";

    public ManifestStatus Status               { get; set; } = ManifestStatus.Borrador;
    public string?        SignedManifestFileName { get; set; }   
    public DateOnly?      SignedDate            { get; set; }   
    public string   ManifestNumber                  { get; set; } = string.Empty;
    public DateOnly ManifestDate                    { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public TimeOnly? ManifestTime                   { get; set; }
    public string   EnvironmentalRegistrationNumber { get; set; } = string.Empty;

    public string   SocialReason             { get; set; } = string.Empty;
    public string   Address                  { get; set; } = string.Empty;   
    public string   Street                   { get; set; } = string.Empty;   
    public string   ExteriorNumber           { get; set; } = string.Empty;
    public string   InteriorNumber           { get; set; } = string.Empty;
    public string   Colony                   { get; set; } = string.Empty;
    public string   PostalCode               { get; set; } = string.Empty;
    public string   Municipality             { get; set; } = string.Empty;
    public string   State                    { get; set; } = string.Empty;
    public string   PhoneNumber              { get; set; } = string.Empty;
    public string   Email                    { get; set; } = string.Empty;
    public DateOnly? GeneratorSignDate       { get; set; }
    public string   GeneratorResponsibleName { get; set; } = string.Empty;
    public string   GeneratorObservations    { get; set; } = string.Empty;
    public string   RegulatoryFramework      { get; set; } = string.Empty;
    public string   SafeHandlingInstructions { get; set; } = string.Empty;

    public List<SpecialResidueItem>   SpecialResidues   { get; set; } = [];
    public List<HazardousResidueItem> HazardousResidues { get; set; } = [];

    public string   TransporterAuthorizationNumber { get; set; } = string.Empty;
    public string   TransporterSCTPermit           { get; set; } = string.Empty;
    public string   TransporterSocialReason        { get; set; } = string.Empty;
    public string   TransporterAddress             { get; set; } = string.Empty; 
    public string   TransporterStreet              { get; set; } = string.Empty; 
    public string   TransporterExteriorNumber      { get; set; } = string.Empty;
    public string   TransporterInteriorNumber      { get; set; } = string.Empty;
    public string   TransporterColony              { get; set; } = string.Empty;
    public string   TransporterPostalCode          { get; set; } = string.Empty;
    public string   TransporterMunicipality        { get; set; } = string.Empty;
    public string   TransporterState               { get; set; } = string.Empty;
    public string   TransporterPhone               { get; set; } = string.Empty;
    public string   TransporterEmail               { get; set; } = string.Empty;
    public string   VehicleType                    { get; set; } = string.Empty;
    public string   VehiclePlate                   { get; set; } = string.Empty;
    public string   DriverName                     { get; set; } = string.Empty;
    public string   DriverLicense                  { get; set; } = string.Empty;
    public string   TransportRoute                 { get; set; } = string.Empty;
    public string   TransporterObservations        { get; set; } = string.Empty;
    public string   TransporterResponsibleName     { get; set; } = string.Empty;
    public DateOnly? TransporterReceptionDate      { get; set; }

    public string   ReceiverAuthorizationNumber { get; set; } = string.Empty;
    public string   ReceiverSocialReason        { get; set; } = string.Empty;
    public string   ReceiverAddress             { get; set; } = string.Empty; 
    public string   ReceiverStreet              { get; set; } = string.Empty; 
    public string   ReceiverExteriorNumber      { get; set; } = string.Empty;
    public string   ReceiverInteriorNumber      { get; set; } = string.Empty;
    public string   ReceiverColony              { get; set; } = string.Empty;
    public string   ReceiverPostalCode          { get; set; } = string.Empty;
    public string   ReceiverMunicipality        { get; set; } = string.Empty;
    public string   ReceiverState               { get; set; } = string.Empty;
    public string   ReceiverPhone               { get; set; } = string.Empty;
    public string   ReceiverEmail               { get; set; } = string.Empty;
    public string   DisposalType                { get; set; } = string.Empty;
    public DateOnly? ReceiverDate               { get; set; }
    public string   ReceiverObservations        { get; set; } = string.Empty;
    public string   ReceiverPersonName          { get; set; } = string.Empty;
    public string   ReceiverResponsibleName     { get; set; } = string.Empty;
}

public class SpecialResidueItem
{
    public string ResidueKey         { get; set; } = string.Empty;
    public string ResidueName        { get; set; } = string.Empty;
    public string Container          { get; set; } = string.Empty;
    public string ContainerCapacity  { get; set; } = string.Empty;
    public float  Amount             { get; set; }
    public string Unit               { get; set; } = "kg";
}

public class HazardousResidueItem
{
    public string ResidueName        { get; set; } = string.Empty;
    public bool   IsCorrosive        { get; set; }
    public bool   IsReactive         { get; set; }
    public bool   IsExplosive        { get; set; }
    public bool   IsToxic            { get; set; }
    public bool   IsFlammable        { get; set; }
    public bool   IsBiological       { get; set; }
    public bool   IsMutagenic        { get; set; }
    public string Container          { get; set; } = string.Empty;
    public string ContainerCapacity  { get; set; } = string.Empty;
    public float  AmountKg           { get; set; }
    public bool?  HasLabel           { get; set; }
}
