using API_WasteCatalog.Models;

namespace API_WasteCatalog.Data;

public static class WasteSeeder
{
    public static void Seed(CatalogDbContext db)
    {
        if (db.WasteTypes.Any()) return;

        db.WasteTypes.AddRange(

            // ════════════════════════════════════════════════════════════════
            // RESIDUOS DE MANEJO ESPECIAL (RME) — Tabla Anexa II
            // Residuos Inorgánicos — SEDEMA Veracruz
            // ════════════════════════════════════════════════════════════════

            new WasteType { Code = "RI-001", Name = "Otros residuos inorgánicos (especificar)",                                                                                     Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-002", Name = "Envases multicapa",                                                                                                            Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RI-003", Name = "Residuos generados en los servicios de transporte y actividades portuarias, aeroportuarias y ferroviarias (incluye llantas)",  Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RI-004", Name = "Residuos de la construcción, mantenimiento y demolición",                                                                      Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RI-005", Name = "Metal ferroso, limalla, colilla de soldadura, escoria, etc.",                                                                  Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-006", Name = "Metal no ferroso",                                                                                                             Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-007", Name = "Fibras sintéticas (trapos y textiles)",                                                                                        Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-008", Name = "Vidrio",                                                                                                                       Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-009", Name = "Fibra de vidrio",                                                                                                              Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-010", Name = "Envases plásticos (especificar)",                                                                                              Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RI-011", Name = "Hule de embalaje",                                                                                                             Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-012", Name = "Hule espuma (poliuretano)",                                                                                                    Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-013", Name = "Unicel (poliestireno)",                                                                                                        Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RI-014", Name = "Residuos de servicios de salud (excluye biológico infecciosos)",                                                               Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RI-015", Name = "Aluminio",                                                                                                                     Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-016", Name = "Residuos tecnológicos y/o electrónicos",                                                                                       Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RI-017", Name = "Residuos de vehículos automotores con características de manejo especial",                                                     Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,pza" },
            new WasteType { Code = "RI-018", Name = "Lana mineral",                                                                                                                 Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-019", Name = "Cerámica y fibra cerámica",                                                                                                    Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-020", Name = "Arcilla",                                                                                                                      Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RI-021", Name = "Carbón activado",                                                                                                              Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RI-022", Name = "Ladrillo refractario",                                                                                                         Type = "especial", LgpgirCategory = "Residuos Inorgánicos", ValidUnits = "kg,ton,pza" },

            // ════════════════════════════════════════════════════════════════
            // RESIDUOS DE MANEJO ESPECIAL (RME) — Tabla Anexa I
            // Residuos Orgánicos — SEDEMA Veracruz
            // ════════════════════════════════════════════════════════════════

            new WasteType { Code = "RO-001", Name = "Otros residuos orgánicos (especificar)",                                                                                                                                                      Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-002", Name = "Papel y cartón",                                                                                                                                                                              Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-003", Name = "Fibra vegetal",                                                                                                                                                                               Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-004", Name = "Madera",                                                                                                                                                                                      Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RO-005", Name = "Hueso",                                                                                                                                                                                       Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-006", Name = "Residuos de actividades agropecuarias",                                                                                                                                                       Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-007", Name = "Residuos alimenticios",                                                                                                                                                                       Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-008", Name = "Cuero",                                                                                                                                                                                       Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-009", Name = "Celulosa",                                                                                                                                                                                    Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-010", Name = "Aceite vegetal comestible y/o grasas",                                                                                                                                                        Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RO-011", Name = "Aguas Residuales (industriales, derivadas de proceso y/o sanitarias)",                                                                                                                        Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "lt,m3" },
            new WasteType { Code = "RO-012", Name = "Lodos de plantas de tratamiento de aguas residuales (deshidratados)",                                                                                                                         Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RO-013", Name = "Lodos provenientes de procesos productivos o actividades diversas de manufactura, y/o provenientes del tratamiento de aguas industriales (deshidratados)",                                    Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RO-014", Name = "Algodón",                                                                                                                                                                                     Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RO-015", Name = "Cenizas y sólidos carbonosos",                                                                                                                                                                Type = "especial", LgpgirCategory = "Residuos Orgánicos", ValidUnits = "kg,ton" },

            // ════════════════════════════════════════════════════════════════
            // RESIDUOS PELIGROSOS (RP) — Lista SIMAR
            // ════════════════════════════════════════════════════════════════

            new WasteType { Code = "RP-001", Name = "Agua contaminada con combustible",                                     Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg,m3" },
            new WasteType { Code = "RP-002", Name = "Agua contaminada con aceite",                                          Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg,m3" },
            new WasteType { Code = "RP-003", Name = "Agua contaminada con pintura",                                         Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg,m3" },
            new WasteType { Code = "RP-004", Name = "Alcoholes",                                                            Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-005", Name = "Arena contaminada con hidrocarburos",                                  Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-006", Name = "Arena contaminada con aceite",                                         Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-007", Name = "Balastras gastadas de lámparas",                                       Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-008", Name = "Baterías alcalinas",                                                   Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-009", Name = "Baterías externas UPS",                                                Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-010", Name = "Baterías usadas",                                                      Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-011", Name = "Cartucho de tóner",                                                    Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-012", Name = "Diluyente envolvente",                                                 Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-013", Name = "Electrónicos",                                                         Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-014", Name = "Envases de plaguicidas",                                               Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-015", Name = "Envases vacíos de metal",                                              Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-016", Name = "Envases vacíos de aerosol",                                            Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-017", Name = "Envases vacíos de plástico",                                           Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-018", Name = "Envases vacíos de vidrio",                                             Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-019", Name = "Filtros automotrices de aceite usados",                                Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-020", Name = "Focos",                                                                Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-021", Name = "Formol",                                                               Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-022", Name = "Lámparas fluorescentes",                                               Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-023", Name = "Lámparas LED",                                                         Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-024", Name = "Lodo con aceite",                                                      Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-025", Name = "Lodo con combustible",                                                 Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-026", Name = "Lodos con percloroetileno",                                            Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-027", Name = "Medicamento caduco sólido",                                            Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg" },
            new WasteType { Code = "RP-028", Name = "Medicamento caduco líquido",                                           Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-029", Name = "Metales pesados",                                                      Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-030", Name = "Pilas alcalinas",                                                      Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,pza" },
            new WasteType { Code = "RP-031", Name = "Pintura caduca",                                                       Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-032", Name = "Sellador caduco",                                                      Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,lt" },
            new WasteType { Code = "RP-033", Name = "Sólidos de plástico contaminados con sustancias químicas",             Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-034", Name = "Sólidos impregnados con solventes",                                    Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-035", Name = "Sólidos impregnados con grasas",                                       Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-036", Name = "Solución ácida",                                                       Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-037", Name = "Solución alcalina",                                                    Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-038", Name = "Solución colorante",                                                   Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-039", Name = "Sustancias inorgánicas",                                               Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton" },
            new WasteType { Code = "RP-040", Name = "Sustancias jabonosas",                                                 Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },
            new WasteType { Code = "RP-041", Name = "Sustancias orgánicas",                                                 Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,lt" },
            new WasteType { Code = "RP-042", Name = "Sustancias reactivas",                                                 Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,lt" },
            new WasteType { Code = "RP-043", Name = "Tierra contaminada",                                                   Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "kg,ton,m3" },
            new WasteType { Code = "RP-044", Name = "Tintas usadas",                                                        Type = "peligroso", LgpgirCategory = "Residuos Peligrosos", ValidUnits = "lt,kg" },

            // ════════════════════════════════════════════════════════════════
            // RESIDUOS PELIGROSOS BIOLÓGICO INFECCIOSOS (RPBI)
            // NOM-087-SEMARNAT-SSA1-2002 — Clasificación B (Biológico)
            // ════════════════════════════════════════════════════════════════

            new WasteType { Code = "RPBI-001", Name = "Cultivos y cepas",          Type = "peligroso", IsBiological = true, LgpgirCategory = "Residuos Peligrosos Biológico Infecciosos", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RPBI-002", Name = "Objetos punzocortantes",     Type = "peligroso", IsBiological = true, LgpgirCategory = "Residuos Peligrosos Biológico Infecciosos", ValidUnits = "kg,contenedores,pza" },
            new WasteType { Code = "RPBI-003", Name = "Residuos patológicos",       Type = "peligroso", IsBiological = true, LgpgirCategory = "Residuos Peligrosos Biológico Infecciosos", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RPBI-004", Name = "Residuos no anatómicos",     Type = "peligroso", IsBiological = true, LgpgirCategory = "Residuos Peligrosos Biológico Infecciosos", ValidUnits = "kg,contenedores" },
            new WasteType { Code = "RPBI-005", Name = "Sangre",                     Type = "peligroso", IsBiological = true, LgpgirCategory = "Residuos Peligrosos Biológico Infecciosos", ValidUnits = "lt,kg,contenedores" }
        );

        db.SaveChanges();
    }
}
