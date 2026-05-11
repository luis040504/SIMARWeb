using API_WasteCatalog.Models;

namespace API_WasteCatalog.Data;

public static class WasteSeeder
{
    public static void Seed(CatalogDbContext db)
    {
        if (db.WasteTypes.Any()) return;

        db.WasteTypes.AddRange(

            // ════════════════════════════════════════════════════════════════
            // RESIDUOS PELIGROSOS (RP) — NOM-052-SEMARNAT-2005
            // Biological-infectious (RPBI) — NOM-087-SEMARNAT-SSA1-2002
            // ════════════════════════════════════════════════════════════════

            new WasteType
            {
                Code = "RP-RPBI-001",
                Name = "Objetos punzocortantes",
                Type = "peligroso",
                Description = "Agujas, jeringas, lancetas, bisturís y cualquier objeto con capacidad de penetrar o cortar tejidos.",
                IsBiological = true, IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,contenedores,pza"
            },
            new WasteType
            {
                Code = "RP-RPBI-002",
                Name = "Residuos no anatómicos",
                Type = "peligroso",
                Description = "Materiales de curación, gasas, torundas, catéteres, guantes y material desechable contaminado con sangre.",
                IsBiological = true, IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,contenedores"
            },
            new WasteType
            {
                Code = "RP-RPBI-003",
                Name = "Residuos anatómicos humanos",
                Type = "peligroso",
                Description = "Tejidos, órganos y partes del cuerpo humano extraídos durante procedimientos quirúrgicos o necropsias.",
                IsBiological = true, IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,contenedores"
            },
            new WasteType
            {
                Code = "RP-RPBI-004",
                Name = "Sangre y derivados líquidos",
                Type = "peligroso",
                Description = "Sangre entera, plasma, suero y otros derivados sanguíneos en forma líquida.",
                IsBiological = true, IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,contenedores"
            },
            new WasteType
            {
                Code = "RP-RPBI-005",
                Name = "Cultivos y cepas de agentes infecciosos",
                Type = "peligroso",
                Description = "Cultivos generados en laboratorios de diagnóstico e investigación, incluyendo medios de cultivo.",
                IsBiological = true, IsReactive = true, IsToxic = true,
                IsCorrosive = false, IsExplosive = false, IsFlammable = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,contenedores"
            },

            // ── RP Industrial ────────────────────────────────────────────────

            new WasteType
            {
                Code = "RP-SOL-001",
                Name = "Solventes halogenados gastados",
                Type = "peligroso",
                Description = "Tricloroetileno, percloroetileno, cloruro de metileno y otros solventes clorados usados.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },
            new WasteType
            {
                Code = "RP-ACE-001",
                Name = "Aceites lubricantes usados",
                Type = "peligroso",
                Description = "Aceites minerales y sintéticos provenientes de motores, transformadores o maquinaria industrial.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },
            new WasteType
            {
                Code = "RP-BAT-001",
                Name = "Baterías de plomo-ácido",
                Type = "peligroso",
                Description = "Baterías automotrices e industriales que contienen ácido sulfúrico y plomo.",
                IsCorrosive = true, IsToxic = true,
                IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-LAM-001",
                Name = "Lámparas fluorescentes con mercurio",
                Type = "peligroso",
                Description = "Tubos y lámparas compactas fluorescentes que contienen vapor de mercurio.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-PIN-001",
                Name = "Pinturas y barnices con solventes orgánicos",
                Type = "peligroso",
                Description = "Pinturas, lacas, barnices y recubrimientos con solventes que presentan características de inflamabilidad y toxicidad.",
                IsFlammable = true, IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },
            new WasteType
            {
                Code = "RP-PLA-001",
                Name = "Plaguicidas caducos y sus envases",
                Type = "peligroso",
                Description = "Herbicidas, insecticidas, fungicidas y raticidas vencidos, así como sus envases contaminados.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,lt"
            },
            new WasteType
            {
                Code = "RP-COR-001",
                Name = "Sustancias ácidas o cáusticas",
                Type = "peligroso",
                Description = "Ácidos minerales (clorhídrico, sulfúrico, nítrico) e hidróxidos (sosa cáustica, cal) en concentraciones peligrosas.",
                IsCorrosive = true, IsToxic = true,
                IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },

            // ════════════════════════════════════════════════════════════════
            // RESIDUOS DE MANEJO ESPECIAL (RME) — LGPGIR Art. 19
            // ════════════════════════════════════════════════════════════════

            new WasteType
            {
                Code = "RME-SAL-001",
                Name = "Residuos no peligrosos de servicios de salud",
                Type = "especial",
                Description = "Residuos generados en unidades médicas que no presentan características de peligrosidad (papel, cartón, plásticos no contaminados).",
                LgpgirCategory = "Servicios de salud",
                ValidUnits = "kg,contenedores"
            },
            new WasteType
            {
                Code = "RME-CAR-001",
                Name = "Cartón y papel",
                Type = "especial",
                Description = "Cajas, embalajes de cartón y papel de oficina, industrial o comercial.",
                LgpgirCategory = "Materiales recuperables",
                ValidUnits = "kg,ton"
            },
            new WasteType
            {
                Code = "RME-PLA-001",
                Name = "Plástico industrial",
                Type = "especial",
                Description = "Envases, empaques y materiales de plástico rígido y flexible de origen industrial o comercial.",
                LgpgirCategory = "Materiales recuperables",
                ValidUnits = "kg,ton"
            },
            new WasteType
            {
                Code = "RME-VID-001",
                Name = "Vidrio roto o desechado",
                Type = "especial",
                Description = "Botellas, frascos y vidrio plano de origen comercial o industrial.",
                LgpgirCategory = "Materiales recuperables",
                ValidUnits = "kg,ton"
            },
            new WasteType
            {
                Code = "RME-MET-001",
                Name = "Chatarra y metales ferrosos",
                Type = "especial",
                Description = "Piezas metálicas, estructuras y equipos fuera de servicio de hierro o acero.",
                LgpgirCategory = "Materiales recuperables",
                ValidUnits = "kg,ton"
            },
            new WasteType
            {
                Code = "RME-LLA-001",
                Name = "Llantas y neumáticos usados",
                Type = "especial",
                Description = "Llantas de vehículos automotores, camiones o maquinaria al término de su vida útil.",
                LgpgirCategory = "Vehículos al final de su vida útil",
                ValidUnits = "pza,kg"
            },
            new WasteType
            {
                Code = "RME-ELE-001",
                Name = "Residuos de aparatos eléctricos y electrónicos (RAEE)",
                Type = "especial",
                Description = "Computadoras, monitores, teléfonos, electrodomésticos y componentes electrónicos desechados.",
                LgpgirCategory = "Tecnológicos",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RME-CON-001",
                Name = "Residuos de construcción y demolición",
                Type = "especial",
                Description = "Concreto, tabique, mezcla, madera de obra y escombro generado en obras civiles.",
                LgpgirCategory = "Construcción",
                ValidUnits = "m3,ton,kg"
            },
            new WasteType
            {
                Code = "RME-RSU-001",
                Name = "Otros residuos inorgánicos (RSU)",
                Type = "especial",
                Description = "Fracción inorgánica de residuos sólidos urbanos no clasificada en otras categorías.",
                LgpgirCategory = "Residuos sólidos urbanos",
                ValidUnits = "kg,ton,m3"
            },
            new WasteType
            {
                Code = "RME-MAD-001",
                Name = "Madera, pallets y residuos forestales",
                Type = "especial",
                Description = "Tarimas, cajas de madera, aserrín y residuos de actividades forestales o de ebanistería.",
                LgpgirCategory = "Forestales",
                ValidUnits = "kg,m3"
            },

            // ════════════════════════════════════════════════════════════════
            // RESIDUOS AUTOMOTRICES — Catálogo AMDA / NOM-052-SEMARNAT-2005
            // Área de servicio
            // ════════════════════════════════════════════════════════════════

            new WasteType
            {
                Code = "RP-AUT-001",
                Name = "Recipientes vacíos de plástico (aceite, anticongelante, líquido de frenos)",
                Type = "peligroso",
                Description = "Envases plásticos que contuvieron aceite, anticongelante o líquido de frenos. Peligrosos por toxicidad ambiental debida a residuos de hidrocarburos y glicoles.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-002",
                Name = "Filtros de aceite usados",
                Type = "peligroso",
                Description = "Cartuchos filtrantes usados impregnados de aceite lubricante gastado con destilados de petróleo, solventes parafínicos y aceite residual hidrogenado.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-003",
                Name = "Filtros de gasolina usados",
                Type = "peligroso",
                Description = "Filtros de combustible con impurezas, óxido del tanque y metales pesados. Presentan toxicidad ambiental por los contaminantes impregnados.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-004",
                Name = "Material sólido impregnado con aceite (trapos, estopas)",
                Type = "peligroso",
                Description = "Fibras textiles saturadas de aceite lubricante usado. Presentan toxicidad ambiental e inflamabilidad por los hidrocarburos absorbidos.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg"
            },
            new WasteType
            {
                Code = "RP-AUT-005",
                Name = "Recipientes vacíos metálicos (aceite, aerosoles)",
                Type = "peligroso",
                Description = "Latas y recipientes metálicos que contuvieron aceite o aerosoles. Contienen matriz sólida porosa peligrosa y residuos de hidrocarburos.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-006",
                Name = "Residuos de anticongelante",
                Type = "peligroso",
                Description = "Anticongelante gastado con metales pesados (plomo, cadmio, cromo) y glicoles. Tóxico ambiental y dañino para la salud por compuestos orgánicos volátiles.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },
            new WasteType
            {
                Code = "RP-AUT-007",
                Name = "Residuos de líquido de frenos",
                Type = "peligroso",
                Description = "Líquido de frenos usado con trazas de metales pesados y glicoles. Presenta toxicidad ambiental y puede provocar desequilibrio ecológico.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },
            new WasteType
            {
                Code = "RP-AUT-008",
                Name = "Desengrasante contaminado (lavado de piezas)",
                Type = "peligroso",
                Description = "Mezcla de hidrocarburos o desengrasante industrial saturado de aceite y grasas. Contiene compuestos orgánicos volátiles (COVs) con inflamabilidad y toxicidad acuática.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },
            new WasteType
            {
                Code = "RP-AUT-009",
                Name = "Convertidores catalíticos gastados",
                Type = "peligroso",
                Description = "Convertidores catalíticos agotados con malla cerámica revestida de platino, rodio y paladio. Peligrosos por toxicidad y reactividad de los metales pesados.",
                IsToxic = true, IsReactive = true,
                IsCorrosive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-010",
                Name = "Gasolina y diésel gastados o sucios",
                Type = "peligroso",
                Description = "Combustibles contaminados o fuera de especificaciones. Altamente inflamables y tóxicos; contienen benceno (cancerígeno) y son peligro para la vida acuática.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },

            // ── Área de hojalatería y pintura ────────────────────────────────

            new WasteType
            {
                Code = "RP-AUT-011",
                Name = "Recipientes vacíos (pintura base solvente o thinner)",
                Type = "peligroso",
                Description = "Envases que contuvieron pintura con solvente o thinner. Contienen metales pesados de pigmentos (plomo, cromo, cadmio) y residuos de solventes inflamables.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-012",
                Name = "Trapos impregnados con solvente o pintura base cromo/plomo",
                Type = "peligroso",
                Description = "Material fibroso contaminado con pintura que contiene cromo hexavalente y plomo, y solventes orgánicos inflamables. Tóxico e inflamable.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg"
            },
            new WasteType
            {
                Code = "RP-AUT-013",
                Name = "Filtros usados de cabina de pintura",
                Type = "peligroso",
                Description = "Filtros saturados de cabina de pintura con residuos de pintura que contienen cromo hexavalente, plomo y cadmio en los pigmentos.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-014",
                Name = "Solventes sucios de lavado de pistolas neumáticas",
                Type = "peligroso",
                Description = "Disolvente orgánico usado en la limpieza de pistolas de pintura, saturado con residuos de pintura con metales pesados y COVs inflamables.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },
            new WasteType
            {
                Code = "RP-AUT-015",
                Name = "Lodos de cabinas de pintura",
                Type = "peligroso",
                Description = "Lodos generados en lavadores de cabinas de pintura con residuos de pintura, barniz y hasta 10% de COVs. Contienen plomo, cromo y cadmio de los pigmentos.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "pastoso",
                ValidUnits = "kg,ton"
            },
            new WasteType
            {
                Code = "RP-AUT-016",
                Name = "Carbón activado agotado de cabina de pintura",
                Type = "peligroso",
                Description = "Filtros de carbón activado de sistemas de extracción que retienen COVs del proceso de pintado. Inflamable por compuestos orgánicos y tóxico por pigmentos metálicos.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg"
            },
            new WasteType
            {
                Code = "RP-AUT-017",
                Name = "Disolventes usados en lavado de equipos de proceso",
                Type = "peligroso",
                Description = "Disolventes orgánicos agotados en limpieza de equipos, contaminados con metales pesados y COVs. Presentan inflamabilidad y toxicidad ambiental.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },
            new WasteType
            {
                Code = "RP-AUT-018",
                Name = "Residuos de agentes secantes, lacas y masillas",
                Type = "peligroso",
                Description = "Carboxilatos metálicos (cobalto, manganeso, plomo), lacas de hidrocarburos y masillas de poliéster con peróxido de benzoilo. Inflamables y con COVs tóxicos.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "pastoso",
                ValidUnits = "kg,lt"
            },

            // ── Área de mantenimiento ────────────────────────────────────────

            new WasteType
            {
                Code = "RP-AUT-019",
                Name = "Recipientes vacíos de pintura/solventes (mantenimiento de instalaciones)",
                Type = "peligroso",
                Description = "Envases que contuvieron pintura base solvente usada en mantenimiento de pisos y estructuras. Contienen residuos con plomo, cromo hexavalente y cadmio.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-020",
                Name = "Trapos y estopas impregnados con solvente (mantenimiento)",
                Type = "peligroso",
                Description = "Material fibroso contaminado con solventes orgánicos en operaciones de mantenimiento general. Inflamable y con toxicidad ambiental.",
                IsToxic = true, IsFlammable = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg"
            },
            new WasteType
            {
                Code = "RP-AUT-021",
                Name = "Lodos de drenajes aceitosos y trampas de separación",
                Type = "peligroso",
                Description = "Lodos provenientes de trampas de aceite y drenajes con hidrocarburos, metales pesados y fenoles. Conservan las propiedades tóxicas del aceite usado.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "pastoso",
                ValidUnits = "kg,ton"
            },
            new WasteType
            {
                Code = "RP-AUT-022",
                Name = "Balastros usados",
                Type = "peligroso",
                Description = "Balastros electromagnéticos de lámparas fluorescentes que pueden contener bifenilos policlorados (BPCs) y di-2-etilohexilo ftalato (DEHF). Bioacumulables y posibles cancerígenos.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "solido",
                ValidUnits = "kg,pza"
            },
            new WasteType
            {
                Code = "RP-AUT-023",
                Name = "Agua con aceite (purgas de compresores)",
                Type = "peligroso",
                Description = "Mezcla agua-aceite de purgas de compresores de aire con metales pesados y aceite lubricante oxidado. Forma resinas persistentes en el ambiente.",
                IsToxic = true,
                IsCorrosive = false, IsReactive = false, IsExplosive = false, IsFlammable = false, IsBiological = false, IsMutagenic = false,
                PhysicalState = "liquido",
                ValidUnits = "lt,kg"
            },

            // ════════════════════════════════════════════════════════════════
            // RESIDUOS DE MANEJO ESPECIAL — Clasificación SEMARNAT LGPGIR
            // RR: Residuos de Rocas y Minerales
            // ════════════════════════════════════════════════════════════════

            new WasteType { Code = "RR-1", Name = "Residuos de grava y rocas trituradas", Type = "especial", LgpgirCategory = "Residuos de Rocas y Minerales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RR-2", Name = "Residuos de arena y arcillas", Type = "especial", LgpgirCategory = "Residuos de Rocas y Minerales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RR-3", Name = "Residuos del corte y serrado de piedra", Type = "especial", LgpgirCategory = "Residuos de Rocas y Minerales", ValidUnits = "kg,ton" },
            new WasteType { Code = "RR-4", Name = "Residuos de polvo y arenilla", Type = "especial", LgpgirCategory = "Residuos de Rocas y Minerales", ValidUnits = "kg,ton" },
            new WasteType { Code = "RR-5", Name = "Otros residuos de rocas y minerales", Type = "especial", LgpgirCategory = "Residuos de Rocas y Minerales", ValidUnits = "kg,ton" },

            // ── RSA: Residuos de Servicios de Salud ─────────────────────────

            new WasteType { Code = "RSA-1", Name = "Residuos de salud sin requisitos especiales (NOM-052/085)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RSA-2", Name = "Vendajes y apósitos usados", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RSA-3", Name = "Ropa de uso médico", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg" },
            new WasteType { Code = "RSA-4", Name = "Pañales usados (servicios de salud)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RSA-5", Name = "Toallas sanitarias usadas", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RSA-6", Name = "Abatelenguas y material de madera médica", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RSA-7", Name = "Objetos cortantes y punzocortantes (salud, no RPBI)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RSA-8", Name = "Restos anatómicos no peligrosos", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RSA-9", Name = "Medicamentos no peligrosos (salud)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RSA-10", Name = "Placas radiológicas", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,pza" },
            new WasteType { Code = "RSA-11", Name = "Otros residuos de servicios de salud", Type = "especial", LgpgirCategory = "Residuos de Servicios de Salud", ValidUnits = "kg,contenedores" },

            // ── RGA-PASFA: Residuos Agropecuarios, Pesqueros y Acuícolas ────

            new WasteType { Code = "RGA-PASFA-1", Name = "Lodos de lavado y limpieza de equipos agropecuarios", Type = "especial", LgpgirCategory = "Residuos Agropecuarios, Pesqueros y Acuícolas", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RGA-PASFA-2", Name = "Residuos de tejidos de animales y cadáveres", Type = "especial", LgpgirCategory = "Residuos Agropecuarios, Pesqueros y Acuícolas", ValidUnits = "kg,ton" },
            new WasteType { Code = "RGA-PASFA-3", Name = "Residuos de tejidos de vegetales", Type = "especial", LgpgirCategory = "Residuos Agropecuarios, Pesqueros y Acuícolas", ValidUnits = "kg,ton" },
            new WasteType { Code = "RGA-PASFA-4", Name = "Heces, orina, estiércol y efluentes agropecuarios", Type = "especial", LgpgirCategory = "Residuos Agropecuarios, Pesqueros y Acuícolas", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RGA-PASFA-5", Name = "Residuos agroquímicos no peligrosos", Type = "especial", LgpgirCategory = "Residuos Agropecuarios, Pesqueros y Acuícolas", ValidUnits = "kg,lt" },
            new WasteType { Code = "RGA-PASFA-6", Name = "Objetos cortantes agropecuarios (no RPBI)", Type = "especial", LgpgirCategory = "Residuos Agropecuarios, Pesqueros y Acuícolas", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RGA-PASFA-7", Name = "Medicamentos y hormonas agropecuarias no peligrosas", Type = "especial", LgpgirCategory = "Residuos Agropecuarios, Pesqueros y Acuícolas", ValidUnits = "kg,lt" },
            new WasteType { Code = "RGA-PASFA-8", Name = "Otros residuos agropecuarios, pesqueros y acuícolas", Type = "especial", LgpgirCategory = "Residuos Agropecuarios, Pesqueros y Acuícolas", ValidUnits = "kg,ton" },

            // ── RINP: Residuos Industriales No Peligrosos ───────────────────

            new WasteType { Code = "RINP-1", Name = "Aserrín, virutas, recortes y madera no impregnada", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RINP-2", Name = "Residuos de fibras textiles procesadas y no procesadas", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-3", Name = "Residuos de cueros y pieles curtidas", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-4", Name = "Residuos con metales (previo CRETI)", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-5", Name = "Residuos de pinturas y tintas no peligrosas", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,lt" },
            new WasteType { Code = "RINP-6", Name = "Residuos de tóner de impresión no peligrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RINP-7", Name = "Residuos de adhesivos y sellantes no peligrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,lt" },
            new WasteType { Code = "RINP-8", Name = "Cenizas, escorias y polvo industrial no peligroso", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-9", Name = "Partículas de tratamiento de efluentes gaseosos no peligrosas", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-10", Name = "Escorias de hornos no peligrosas", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-11", Name = "Residuos de materiales de fibra de vidrio", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-12", Name = "Residuos de fibras sintéticas (nylon, poliéster)", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-13", Name = "Residuos de plástico de película y polietileno de baja densidad", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-14", Name = "Residuos de plástico rígido (PET, HDPE, PVC, PP)", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-15", Name = "Residuos de poliuretano", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-16", Name = "Residuos de poliestireno expandido (unicel)", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RINP-17", Name = "Residuos de cerámica, ladrillos y tejas no peligrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-18", Name = "Limaduras y virutas de metales férreos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-19", Name = "Polvo y partículas de metales férreos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-20", Name = "Limaduras y virutas de metales no férreos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-21", Name = "Polvo y partículas de metales no férreos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-22", Name = "Virutas y rebabas de plástico", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-23", Name = "Residuos de soldadura", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-24", Name = "Envases de papel y cartón", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RINP-25", Name = "Envases de plástico", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RINP-26", Name = "Envases de madera", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RINP-27", Name = "Envases metálicos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RINP-28", Name = "Envases de vidrio", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RINP-29", Name = "Envases textiles", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RINP-30", Name = "Envases de hule", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RINP-31", Name = "Absorbentes, trapos y ropas protectoras no peligrosas", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-32", Name = "Neumáticos fuera de uso, hules y similares", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "pza,kg,ton" },
            new WasteType { Code = "RINP-33", Name = "Vehículos al final de vida útil (sin componentes peligrosos)", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "pza,kg,ton" },
            new WasteType { Code = "RINP-34", Name = "Metales ferrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-35", Name = "Metales no ferrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-36", Name = "Residuos de equipos eléctricos y electrónicos no peligrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RINP-37", Name = "Cables no peligrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-38", Name = "Tierras, piedras y lodos de drenaje no peligrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RINP-39", Name = "Residuos de construcción y demolición no peligrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RINP-40", Name = "Bagazo de malta", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-41", Name = "Levadura líquida", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "lt,m3" },
            new WasteType { Code = "RINP-42", Name = "Malta seca", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-43", Name = "Melanina", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-44", Name = "Espaciador de Noryl", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RINP-45", Name = "Acetal", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-46", Name = "Papel encerado", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-47", Name = "Micelio", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RINP-48", Name = "Otros residuos industriales no peligrosos", Type = "especial", LgpgirCategory = "Residuos Industriales No Peligrosos", ValidUnits = "kg,ton" },

            // ── RST-PAFPA: Residuos de Servicios de Transporte ──────────────

            new WasteType { Code = "RST-PAFPA-1", Name = "Llantas fuera de uso (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "pza,kg" },
            new WasteType { Code = "RST-PAFPA-2", Name = "Empaques de hule (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,ton" },
            new WasteType { Code = "RST-PAFPA-3", Name = "Cables (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,ton" },
            new WasteType { Code = "RST-PAFPA-4", Name = "Cartón (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,ton" },
            new WasteType { Code = "RST-PAFPA-5", Name = "Materiales férreos (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,ton" },
            new WasteType { Code = "RST-PAFPA-6", Name = "Materiales no férreos (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,ton" },
            new WasteType { Code = "RST-PAFPA-7", Name = "Plásticos (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,ton" },
            new WasteType { Code = "RST-PAFPA-8", Name = "Madera (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RST-PAFPA-9", Name = "Residuos de pintura (transporte)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,lt" },
            new WasteType { Code = "RST-PAFPA-10", Name = "Vehículos al final de vida útil (transporte, sin componentes peligrosos)", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "pza,kg,ton" },
            new WasteType { Code = "RST-PAFPA-11", Name = "Otros residuos de servicios de transporte", Type = "especial", LgpgirCategory = "Residuos de Servicios de Transporte", ValidUnits = "kg,ton" },

            // ── RLTAR: Lodos de Tratamiento de Aguas Residuales ─────────────

            new WasteType { Code = "RLTAR-1", Name = "Lodos de cárcamos de bombeos", Type = "especial", LgpgirCategory = "Lodos de Tratamiento de Aguas Residuales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RLTAR-2", Name = "Lodos y desechos de rejas y rejillas", Type = "especial", LgpgirCategory = "Lodos de Tratamiento de Aguas Residuales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RLTAR-3", Name = "Lodos del desarenador", Type = "especial", LgpgirCategory = "Lodos de Tratamiento de Aguas Residuales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RLTAR-4", Name = "Lodos de digestores (filtros banda, filtros prensa, lechos de secado)", Type = "especial", LgpgirCategory = "Lodos de Tratamiento de Aguas Residuales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RLTAR-5", Name = "Lodos de mantenimiento de drenajes", Type = "especial", LgpgirCategory = "Lodos de Tratamiento de Aguas Residuales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RLTAR-6", Name = "Lodos de precipitaciones químicas, flotaciones y filtraciones", Type = "especial", LgpgirCategory = "Lodos de Tratamiento de Aguas Residuales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RLTAR-7", Name = "Lodos de mantenimiento de equipos de tratamiento de aguas", Type = "especial", LgpgirCategory = "Lodos de Tratamiento de Aguas Residuales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RLTAR-8", Name = "Otros lodos de tratamiento de aguas residuales", Type = "especial", LgpgirCategory = "Lodos de Tratamiento de Aguas Residuales", ValidUnits = "kg,ton,m3" },

            // ── RDP: Residuos de Tiendas Departamentales y Centros Comerciales

            new WasteType { Code = "RDP-1", Name = "Cartón (tiendas y centros comerciales)", Type = "especial", LgpgirCategory = "Residuos de Tiendas Departamentales", ValidUnits = "kg,ton" },
            new WasteType { Code = "RDP-2", Name = "Madera (tiendas y centros comerciales)", Type = "especial", LgpgirCategory = "Residuos de Tiendas Departamentales", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RDP-3", Name = "Plásticos (tiendas y centros comerciales)", Type = "especial", LgpgirCategory = "Residuos de Tiendas Departamentales", ValidUnits = "kg,ton" },
            new WasteType { Code = "RDP-4", Name = "Empaques y embalajes (comercio)", Type = "especial", LgpgirCategory = "Residuos de Tiendas Departamentales", ValidUnits = "kg,ton" },
            new WasteType { Code = "RDP-5", Name = "Fibras textiles (tiendas y centros comerciales)", Type = "especial", LgpgirCategory = "Residuos de Tiendas Departamentales", ValidUnits = "kg,ton" },
            new WasteType { Code = "RDP-6", Name = "Otros residuos de tiendas departamentales", Type = "especial", LgpgirCategory = "Residuos de Tiendas Departamentales", ValidUnits = "kg,ton" },

            // ── RC: Residuos de Construcción, Mantenimiento y Demolición ────

            new WasteType { Code = "RC-1", Name = "Adocretos", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "pza,kg,ton" },
            new WasteType { Code = "RC-2", Name = "Concretos limpios", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RC-3", Name = "Concreto armado", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RC-4", Name = "Mampostería", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RC-5", Name = "Tepetates", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RC-6", Name = "Tabiques", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "pza,kg,ton" },
            new WasteType { Code = "RC-7", Name = "Ladrillos", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "pza,kg,ton" },
            new WasteType { Code = "RC-8", Name = "Blocks", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "pza,kg,ton" },
            new WasteType { Code = "RC-9", Name = "Morteros", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RC-10", Name = "Suelo orgánico", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RC-11", Name = "Suelo y materiales arcillosos, granulares y pétreos no contaminados", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RC-12", Name = "Otros materiales de construcción y demolición", Type = "especial", LgpgirCategory = "Residuos de Construcción y Demolición", ValidUnits = "kg,ton,m3" },

            // ── RTPII: Residuos Tecnológicos ─────────────────────────────────

            new WasteType { Code = "RTPII-1", Name = "Cartón (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-2", Name = "Hule (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-3", Name = "Papel (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-4", Name = "Unicel (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RTPII-5", Name = "Cables (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-6", Name = "Materiales ferrosos (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-7", Name = "Materiales no ferrosos (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-8", Name = "Plásticos (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-9", Name = "Fibras textiles (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-10", Name = "Piel (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-11", Name = "Fibras sintéticas (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-12", Name = "Poliuretano (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RTPII-13", Name = "Madera (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RTPII-14", Name = "Envases (residuos tecnológicos)", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RTPII-15", Name = "Otros residuos tecnológicos", Type = "especial", LgpgirCategory = "Residuos Tecnológicos", ValidUnits = "kg,ton" },

            // ── RO: Residuos Sólidos Urbanos Orgánicos ───────────────────────

            new WasteType { Code = "RO-1", Name = "Residuos de alimentos de cocinas y restaurantes", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-2", Name = "Aceites y grasas comestibles", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RO-3", Name = "Residuos de parques y jardines (podas, hojarascas)", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RO-4", Name = "Bagazo", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-5", Name = "Nixtamal", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-6", Name = "Cascarillas de café, cacao, nuez y aguacate", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-7", Name = "Huesos", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-8", Name = "Chocolate (residuos)", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-9", Name = "Otros residuos sólidos urbanos orgánicos", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Orgánicos", ValidUnits = "kg,ton" },

            // ── RI: Residuos Sólidos Urbanos Inorgánicos ────────────────────

            new WasteType { Code = "RI-1", Name = "Papel (periódico, oficinas, empaques, revistas)", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-2", Name = "Cartón (empaques lisos, rugosos, envases de leche y jugo)", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-3", Name = "Vidrio de color", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-4", Name = "Vidrio transparente", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-5", Name = "Ropa, trapos y similares no impregnados con sustancias peligrosas", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-6", Name = "Madera no impregnada con sustancias peligrosas", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RI-7", Name = "Plásticos (PET, LDPE, poliuretano, poliestireno) de consumo", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-8", Name = "Loza y cerámica (platos, tazas, jarras, ollas)", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-9", Name = "Metales ferrosos de consumo (latas, utensilios de cocina)", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-10", Name = "Metales no ferrosos de consumo (latas de aluminio)", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-11", Name = "Otros residuos sólidos urbanos inorgánicos", Type = "especial", LgpgirCategory = "Residuos Sólidos Urbanos Inorgánicos", ValidUnits = "kg,ton" }
        );

        db.SaveChanges();
    }
}
