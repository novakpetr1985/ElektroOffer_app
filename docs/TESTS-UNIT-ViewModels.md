`CalculationItemViewModelTests_CascadeMaterial.cs`
| Typ testu       | Číslo    | Název testu                                                                                  | Popis                                       | Poznámka |
| --------------- | -------- | -------------------------------------------------------------------------------------------- | ------------------------------------------- | -------- |
| CascadeMaterial | **T_001** | T_001_CascadeMaterial_ResetBelowCategory_Should_Clear_Names_Suppliers_Offers                 | Reset Name + Supplier + Offer               | ⭐ NOVÝ |
| CascadeMaterial | **T_002** | T_002_CascadeMaterial_ResetBelowMaterialName_Should_Clear_Suppliers_And_Offers               | Reset Supplier + Offer                      | ⭐ NOVÝ |
| CascadeMaterial | **T_003** | T_003_CascadeMaterial_ResetBelowSupplier_Should_Clear_Offers                                 | Reset Offer                                 | ⭐ NOVÝ |
| CascadeMaterial | **T_004** | T_004_CascadeMaterial_SelectedCategory_Should_Raise_PropertyChanged_For_CanSelectProductName | Category → CanSelectProductName             | ⭐ NOVÝ |
| CascadeMaterial | **T_005** | T_005_CascadeMaterial_SelectedProductName_Should_Raise_PropertyChanged_For_CanSelectSupplier | ProductName → CanSelectSupplier             | ⭐ NOVÝ |
| CascadeMaterial | **T_006** | T_006_CascadeMaterial_SelectedSupplier_Should_Raise_PropertyChanged_For_CanSelectOffer       | Supplier → CanSelectOffer                   | ⭐ NOVÝ |
| CascadeMaterial | **T_007** | **T_007_CascadeMaterial_SelectedOffer_Should_Update_SelectedMaterialPrice**                  | Výběr nabídky nastaví SelectedMaterialPrice | ⭐ NOVÝ |

`CalculationItemViewModelTests_CascadeWork.cs`
| Typ testu   | Číslo    | Název testu                                                                                | Popis                             | Poznámka |
| ----------- | -------- | ------------------------------------------------------------------------------------------ | --------------------------------- | -------- |
| CascadeWork | **T_008** | T_008_CascadeWork_ResetBelowTask_Should_Clear_All_Selections                               | Reset Specification               | ⭐ NOVÝ |
| CascadeWork | **T_009** | T_009_CascadeWork_ResetBelowSpecification_Should_Clear_Material_And_Location               | Reset Material + Location         | ⭐ NOVÝ |
| CascadeWork | **T_010** | T_010_CascadeWork_ResetBelowMaterial_Should_Clear_Location                                 | Reset Location                    | ⭐ NOVÝ |
| CascadeWork | **T_011** | T_011_CascadeWork_SelectedTask_Should_Raise_PropertyChanged_For_CanSelectSpecification     | Task → CanSelectSpecification     | ⭐ NOVÝ |
| CascadeWork | **T_012** | T_012_CascadeWork_SelectedSpecification_Should_Raise_PropertyChanged_For_CanSelectMaterial | Specification → CanSelectMaterial | ⭐ NOVÝ |
| CascadeWork | **T_013** | T_013_CascadeWork_SelectedMaterial_Should_Raise_PropertyChanged_For_CanSelectLocation      | Material → CanSelectLocation      | ⭐ NOVÝ |
| CascadeWork | **T_014** | T_014_CascadeWork_SelectedLocation_Should_Update_WorkItem                                  | Location → WorkItem               | ⭐ NOVÝ |


`CalculationItemViewModelTests_IsEmpty.cs`
| Typ testu   | Číslo    | Název testu                                                       | Popis                                | Poznámka |
| ----------- | -------- | ----------------------------------------------------------------- | ------------------------------------ | -------- |
| IsEmpty | **T_015** | T_029_IsEmpty_Should_Return_True_For_Completely_Empty_Row            | Prázdný řádek → IsEmpty true         | ⭐ NOVÝ |
| IsEmpty | **T_016** | T_030_IsEmpty_Should_Return_False_When_WorkItem_Is_Selected          | WorkItem → IsEmpty false             | ⭐ NOVÝ |
| IsEmpty | **T_017** | T_031_IsEmpty_Should_Return_False_When_MaterialItem_Is_Selected      | MaterialItem → IsEmpty false         | ⭐ NOVÝ |
| IsEmpty | **T_018** | T_048_IsEmpty_Should_Handle_Combination_Of_Values                    | Kombinace hodnot                     | ⭐ NOVÝ |
| IsEmpty | **T_019** | T_066_IsEmpty_Should_Return_False_When_Quantity_Is_Greater_Than_Zero | Quantity > 0 → IsEmpty false         | ⭐ NOVÝ |
| IsEmpty | **T_020** | T_067_IsEmpty_Should_Return_False_When_Location_Is_Selected          | Location → IsEmpty false             | ⭐ NOVÝ |
| IsEmpty | **T_021** | T_068_IsEmpty_Should_Return_False_When_Task_Is_Selected              | Task → IsEmpty false                 | ⭐ NOVÝ |
| IsEmpty | **T_022** | T_022_IsEmpty_Should_Be_True_When_All_Fields_Are_Default             | Default hodnoty → IsEmpty true       | ⭐ NOVÝ |
| IsEmpty | **T_023** | T_023_IsEmpty_Should_Be_False_When_Task_Is_Selected                  | Task → IsEmpty false                 | ⭐ NOVÝ |
| IsEmpty | **T_024** | T_024_IsEmpty_Should_Be_False_When_Quantity_Is_Positive              | Quantity > 0 → IsEmpty false         | ⭐ NOVÝ |
| IsEmpty | **T_025** | T_025_IsEmpty_Should_Be_False_When_Discount_Is_Enabled               | Sleva + Quantity > 0 → IsEmpty false | ⭐ NOVÝ |
| IsEmpty | **T_026** | T_026_IsEmpty_Should_Be_False_When_Material_Is_Selected              | Material → IsEmpty false             | ⭐ NOVÝ |
| IsEmpty | **T_027** | T_027_IsEmpty_Should_Be_False_When_Location_Is_Selected              | Location → IsEmpty false             | ⭐ NOVÝ |


`CalculationItemViewModelTests_PropertyChanged.cs`
| Typ testu       | Číslo    | Název testu                                                                                | Popis                              | Poznámka |
| --------------- | -------- | ------------------------------------------------------------------------------------------ | ---------------------------------- | -------- |
| PropertyChanged | **T_028** | T_028_PropertyChanged_Total_Should_Ignore_Negative_Discount                                | Negativní sleva se ignoruje        |    -    |
| PropertyChanged | **T_029** | T_029_PropertyChanged_Quantity_Should_Raise_PropertyChanged_For_Total                      | Quantity vyvolá Total              |    -    |
| PropertyChanged | **T_030** | T_030_PropertyChanged_WorkItem_Should_Raise_PropertyChanged_For_Total                      | WorkItem vyvolá Total              |    -    |
| PropertyChanged | **T_031** | T_031_PropertyChanged_MaterialItem_Should_Raise_PropertyChanged_For_Total                  | MaterialItem vyvolá Total          |    -    |
| PropertyChanged | **T_032** | T_032_PropertyChanged_DiscountPercent_Should_Raise_PropertyChanged_For_Total               | Sleva vyvolá Total                 |    -    |
| PropertyChanged | **T_033** | T_033_PropertyChanged_IsEmpty_Should_Raise_PropertyChanged_When_Inputs_Change              | Změna vstupů vyvolá IsEmpty        | ⭐ NOVÝ |
| PropertyChanged | **T_034** | T_034_PropertyChanged_DiscountEnabled_Should_Raise_PropertyChanged_For_IsEmpty             | Sleva vyvolá IsEmpty               | ⭐ NOVÝ |
| PropertyChanged | **T_035** | T_035_PropertyChanged_Quantity_Should_Raise_PropertyChanged_For_Total_Only_Once            | Quantity vyvolá Total jednou       | ⭐ NOVÝ |
| PropertyChanged | **T_036** | T_036_PropertyChanged_DiscountPercent_Should_Raise_PropertyChanged_For_Total_Only_Once     | Sleva vyvolá Total jednou          | ⭐ NOVÝ |
| PropertyChanged | **T_037** | T_037_PropertyChanged_SelectedTask_Should_Raise_PropertyChanged_For_IsEmpty                | Task vyvolá IsEmpty                | ⭐ NOVÝ |
| PropertyChanged | **T_038** | T_038_PropertyChanged_SelectedMaterial_Should_Raise_PropertyChanged_For_IsEmpty            | Material vyvolá IsEmpty            | ⭐ NOVÝ |
| PropertyChanged | **T_039** | T_039_PropertyChanged_SelectedLocation_Should_Raise_PropertyChanged_For_Total              | Location vyvolá Total              | ⭐ NOVÝ |
| PropertyChanged | **T_040** | T_040_PropertyChanged_IsDiscountEnabled_Should_Raise_PropertyChanged_For_Total_And_IsEmpty | Sleva vyvolá Total + IsEmpty       | ⭐ NOVÝ |
| PropertyChanged | **T_041** | T_041_SelectedMaterialPrice_Should_Raise_PropertyChanged_For_Total_Only_Once               | Cena materiálu vyvolá Total jednou | ⭐ NOVÝ |
| PropertyChanged | **T_042** | T_042_PropertyChanged_WorkItem_Should_Raise_PropertyChanged_For_Total_Only_Once            | WorkItem vyvolá Total jednou       | ⭐ NOVÝ |


`CalculationItemViewModelTests_Total.cs`
| Typ testu | Číslo | Název testu                                                                      | Popis                                      | Poznámka |
| ----- | --------- | -------------------------------------------------------------------------------- | ------------------------------------------ | -------- |
| Total | **T_043** | T_043_Total_Should_Calculate_WorkItem_Correctly                                  | Výpočet Total z WorkItem                   |    -    |
| Total | **T_044** | T_044_Total_Should_Calculate_MaterialItem_When_WorkItem_Is_Null                  | Výpočet Total z MaterialItem               |    -    |
| Total | **T_045** | T_045_Total_Should_Apply_Discount                                                | Sleva se správně aplikuje                  |    -    |
| Total | **T_046** | T_046_Total_Should_Not_Apply_Discount_When_Disabled                              | Sleva se neaplikuje, když je vypnutá       |    -    |
| Total | **T_047** | T_047_IsDiscountEnabled_False_Should_Reset_DiscountPercent                       | Vypnutí slevy resetuje DiscountPercent     |    -    |
| Total | **T_048** | T_048_Total_Should_Respect_Material_And_Position_Coefs                           | Výpočet respektuje koeficienty             |    -    |
| Total | **T_049** | T_049_Total_Should_Be_Zero_When_Quantity_Is_Zero                                 | Quantity = 0 → Total = 0                   |    -    |
| Total | **T_050** | T_050_Total_Should_Be_Zero_When_BasePrice_Is_Zero                                | BasePrice = 0 → Total = 0                  |    -    |
| Total | **T_051** | T_051_Total_Should_Be_Zero_When_WorkItem_And_MaterialItem_Are_Null               | Bez vstupů → Total = 0                     |    -    |
| Total | **T_052** | T_052_Total_Should_Use_WorkItem_When_Both_WorkItem_And_MaterialItem_Are_Set      | WorkItem má prioritu                       |    -    |
| Total | **T_053** | T_053_Total_Should_Be_Zero_When_Discount_Is_100_Percent                          | Sleva 100 % → Total = 0                    |    -    |
| Total | **T_054** | T_054_Total_Should_Not_Be_Negative_When_Discount_Above_100                       | Sleva >100 % → Total >= 0                  |    -    |
| Total | **T_055** | T_055_Total_Should_Apply_Discount_On_WorkItem                                    | Sleva na WorkItem                          | ⭐ NOVÝ |
| Total | **T_056** | T_056_Total_Should_Apply_Discount_On_MaterialItem_When_WorkItem_Is_Null          | Sleva na MaterialItem                      | ⭐ NOVÝ |
| Total | **T_057** | T_057_SelectedMaterialPrice_Should_Raise_PropertyChanged_For_Total               | Změna ceny materiálu vyvolá Total          | ⭐ NOVÝ |
| Total | **T_058** | T_058_Total_Should_Handle_Extreme_Quantity_Values                                | Extrémní Quantity                          | ⭐ NOVÝ |
| Total | **T_059** | T_059_Total_Should_Be_Zero_When_SelectedMaterialPrice_Is_Null                    | Null cena materiálu → Total = 0            | ⭐ NOVÝ |
| Total | **T_060** | T_060_Total_Should_Update_When_SelectedMaterialPrice_Changes                     | Změna ceny materiálu přepočítá Total       | ⭐ NOVÝ |
| Total | **T_061** | T_051_Total_Should_Handle_Extreme_Material_Prices                                | Extrémní ceny materiálu                    | ⭐ NOVÝ |
| Total | **T_062** | T_062_Total_Should_Round_Correctly_To_Three_Decimals                             | Zaokrouhlení na 3 desetinná místa          | ⭐ NOVÝ |
| Total | **T_063** | T_063_Total_Should_Recalculate_Correctly_After_Multiple_Changes                  | Více změn → správný přepočet               | ⭐ NOVÝ |
| Total | **T_064** | T_064_Total_Should_Not_Throw_When_WorkItem_Is_Null_And_MaterialItem_Is_Valid     | WorkItem null, MaterialItem validní        | ⭐ NOVÝ |
| Total | **T_065** | T_065_Total_Should_Use_Price_From_Database_When_MaterialItem_Is_Loaded           | Cena z DB                                  | ⭐ NOVÝ |
| Total | **T_066** | T_066_Total_Should_Switch_From_WorkItem_To_MaterialItem_When_WorkItem_Is_Cleared | Přepnutí WorkItem → MaterialItem           | ⭐ NOVÝ |
| Total | **T_067** | T_067_Discount_Should_Reset_When_DiscountPercent_Is_Set_To_Null                  | Null sleva resetuje                        | ⭐ NOVÝ |
| Total | **T_068** | T_068_Total_Should_Handle_WorkItem_Then_MaterialItem_Then_WorkItem               | Přepínání typů položek                     | ⭐ NOVÝ |
| Total | **T_069** | T_069_Total_Should_Not_Throw_On_Invalid_Values                                   | Robustnost výpočtu                         | ⭐ NOVÝ |
| Total | **T_070** | T_070_Total_Should_Update_When_WorkItem_Coefs_Change                             | Změna koeficientů → Total                  | ⭐ NOVÝ |
| Total | **T_071** | T_071_Total_Should_Use_New_MaterialPrice_When_Supplier_Changes                   | Změna dodavatele → nová cena               | ⭐ NOVÝ |
| Total | **T_072** | T_072_Total_Should_Reset_When_All_Inputs_Are_Cleared                             | Reset vstupů → Total = 0                   | ⭐ NOVÝ |
| Total | **T_073** | T_073_Total_Should_Handle_MaterialPrice_Change_After_WorkItem_Is_Set             | Materiál po WorkItem                       | ⭐ NOVÝ |
| Total | **T_074** | T_074_Total_Should_Handle_Discount_Toggle_Correctly                              | Přepínání slevy                            | ⭐ NOVÝ |
| Total | **T_075** | T_075_Total_Should_Apply_Discount_On_Material_When_WorkItem_Is_Cleared           | Sleva na materiál                          | ⭐ NOVÝ |
| Total | **T_076** | T_076_Total_Should_Handle_Material_Then_WorkItem_Then_Discount                   | Kombinace vstupů                           | ⭐ NOVÝ |
| Total | **T_077** | T_077_Total_Should_Use_Highest_MaterialPrice_From_Database                       | Nejvyšší cena z DB                         | ⭐ NOVÝ |
| Total | **T_078** | T_078_Total_Should_Handle_Discount_After_MaterialPrice_Change                    | Sleva po změně ceny                        | ⭐ NOVÝ |
| Total | **T_079** | T_079_Total_Should_Handle_WorkItem_After_Discount_Change                         | WorkItem po změně slevy                    | ⭐ NOVÝ |
| Total | **T_080** | T_080_Total_Should_Handle_MaxInt_Quantity                                        | Max int Quantity                           | ⭐ NOVÝ |
| Total | **T_081** | T_081_Total_Should_Handle_MaxDouble_BasePrice                                    | Max double cena                            | ⭐ NOVÝ |
| Total | **T_082** | T_082_Total_Should_Handle_MaterialPrice_Null_Then_NotNull                        | Null → NotNull                             | ⭐ NOVÝ |
| Total | **T_083** | T_083_Total_Should_Handle_WorkItem_Null_Then_NotNull                             | WorkItem null → not null                   | ⭐ NOVÝ |
| Total | **T_084** | T_084_Total_Should_Handle_Discount_Null_Then_NotNull                             | Sleva null → not null                      | ⭐ NOVÝ |
| Total | **T_085** | T_085_Total_Should_Handle_Discount_NotNull_Then_Null                             | Sleva not null → null                      | ⭐ NOVÝ |
| Total | **T_086** | T_086_Total_Should_Handle_MaterialPrice_Change_With_Discount                     | Cena + sleva                               | ⭐ NOVÝ |
| Total | **T_087** | T_087_Total_Should_Handle_WorkItem_Change_With_Discount                          | WorkItem + sleva                           | ⭐ NOVÝ |
| Total | **T_088** | T_088_Total_Should_Handle_All_Inputs_Changing_At_Once                            | Všechny vstupy najednou                    | ⭐ NOVÝ |
| Total | **T_089** | T_089_Total_Should_Handle_WorkItem_With_MaterialPrice_And_Discount               | Kombinace WorkItem + MaterialPrice + sleva | ⭐ NOVÝ |
| Total | **T_090** | T_090_Total_Should_Handle_MaterialPrice_With_Discount_And_Quantity_Change        | Materiál + sleva + Quantity                | ⭐ NOVÝ |
| Total | **T_091** | T_091_Total_Should_Not_Throw_When_MaterialPrice_Is_Deleted_From_DB               | Robustnost při smazání ceny                | ⭐ NOVÝ |
| Total | **T_092** | T_092_Total_Should_Update_When_MaterialPrice_Is_Updated_In_DB                    | Aktualizace ceny v DB                      | ⭐ NOVÝ |
| Total | **T_093** | T_093_Total_Should_Use_WorkItem_When_MaterialPrice_Is_Set                        | WorkItem má prioritu                       | ⭐ NOVÝ |
| Total | **T_094** | T_094_Total_Should_Use_MaterialPrice_When_WorkItem_Is_Cleared                    | MaterialPrice má prioritu                  | ⭐ NOVÝ |
| Total | **T_095** | T_096_Total_Should_Handle_FloatingPoint_EdgeCases                                | FP edge cases                              | ⭐ NOVÝ |
| Total | **T_096** | T_096_Total_Should_Handle_Very_Small_Prices                                      | Malé ceny                                  | ⭐ NOVÝ |


`CalculationItemViewModelTests_Validation.cs`
| Typ testu   | Číslo   | Název testu                                                         | Popis                       | Poznámka |
| ---------- | -------- | ------------------------------------------------------------------- | --------------------------- | -------- |
| Validation | **T_097** | T_097_Validation_Quantity_Should_Clamp_Negative_To_Zero             | Quantity clamp              | ⭐ NOVÝ |
| Validation | **T_098** | T_098_Validation_DiscountPercent_Should_Clamp_Above_100_To_100      | Sleva clamp 100             | ⭐ NOVÝ |
| Validation | **T_099** | T_099_Validation_DiscountPercent_Should_Clamp_Negative_To_Zero      | Sleva clamp 0               | ⭐ NOVÝ |
| Validation | **T_100** | T_100_Validation_SelectedMaterialPrice_Should_Fallback_When_Invalid | Fallback při nevalidní ceně | ⭐ NOVÝ |