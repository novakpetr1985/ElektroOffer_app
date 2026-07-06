
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ElektroOffer_app.Data;
using ElektroOffer_app.Models;

namespace ElektroOffer_app.Services.DataImport
{
    // =========================================================================
    // 🔄 MaterialImportService – zápis dat z Import_Master do databáze
    // =========================================================================
    //
    // K čemu slouží:
    // - Vezme seznam Import (výstup z ImportCsvReader) a zapíše/aktualizuje
    //   odpovídající záznamy Material, Category, Supplier a MaterialPrice
    //   v databázi
    // - Funguje jako "upsert" (update nebo insert) - lze ho SPOUŠTĚT
    //   OPAKOVANĚ (např. při každém novém ceníku od dodavatele) beze
    //   strachu z duplicit:
    //   - Material se najde/vytvoří podle Name (kanonický název)
    //   - Category se najde/vytvoří podle Name
    //   - MaterialPrice se najde podle (SupplierId, SupplierCode) -
    //     pokud existuje, jen se aktualizuje cena; pokud ne, vytvoří se nově
    //
    // Použití DI (Dependency Injection):
    // - AppDbContext se předává přes konstruktor (stejný princip jako
    //   u ProjectService), NE přes "new AppDbContext()" uvnitř metod -
    //   viz naše dřívější poučení o bugu s izolací DB kontextu
    // =========================================================================
    public class MaterialImportService
    {
        private readonly AppDbContext _db;

        public MaterialImportService(AppDbContext db)
        {
            _db = db;
        }

        // ---------------------------------------------------------------
        // Hlavní vstupní bod - naimportuje celý seznam řádků z CSV
        // ---------------------------------------------------------------
        public async Task ImportujMaterialyAsync(List<Import> radky)
        {
            // Dodavatele (ELKOV, EMAS) založíme jen JEDNOU - při dalších
            // spuštěních importu se najdou už existující a znovu se nezakládají
            var elkov = await NajdiNeboVytvorSupplieraAsync("ELKOV");
            var emas = await NajdiNeboVytvorSupplieraAsync("EMAS");

            foreach (var radek in radky)
            {
                var kategorie = await NajdiNeboVytvorKategoriiAsync(radek.Kategorie);
                var material = await NajdiNeboVytvorMaterialAsync(radek, kategorie.Id);

                await UpsertCenuAsync(material.Id, elkov.Id, radek.ElkovKod, radek.ElkovNazev, radek.ElkovMJ, radek.ElkovCena, radek.ElkovMena);
                await UpsertCenuAsync(material.Id, emas.Id, radek.EmasKod, radek.EmasNazev, radek.EmasMJ, radek.EmasCena, radek.EmasMena);
            }
        }

        // ---------------------------------------------------------------
        // Najde dodavatele podle jména, nebo ho založí, pokud ještě neexistuje
        // ---------------------------------------------------------------
        private async Task<Supplier> NajdiNeboVytvorSupplieraAsync(string nazev)
        {
            var supplier = await _db.Suppliers.FirstOrDefaultAsync(s => s.Name == nazev);
            if (supplier == null)
            {
                supplier = new Supplier { Name = nazev };
                _db.Suppliers.Add(supplier);
                await _db.SaveChangesAsync(); // uloženo hned, ať má supplier.Id přiděleno
            }
            return supplier;
        }

        // ---------------------------------------------------------------
        // Najde kategorii podle jména, nebo ji založí, pokud ještě neexistuje
        // ---------------------------------------------------------------
        private async Task<Category> NajdiNeboVytvorKategoriiAsync(string nazev)
        {
            var kategorie = await _db.Categories.FirstOrDefaultAsync(c => c.Name == nazev);
            if (kategorie == null)
            {
                kategorie = new Category { Name = nazev };
                _db.Categories.Add(kategorie);
                await _db.SaveChangesAsync();
            }
            return kategorie;
        }

        // ---------------------------------------------------------------
        // Najde Material podle NAŠEHO kanonického názvu (radek.Nazev),
        // nebo ho založí, pokud ještě v databázi neexistuje
        // ---------------------------------------------------------------
        private async Task<Material> NajdiNeboVytvorMaterialAsync(Import radek, int categoryId)
        {
            var material = await _db.Materials.FirstOrDefaultAsync(m => m.Name == radek.Nazev);
            if (material == null)
            {
                material = new Material
                {
                    Name = radek.Nazev,
                    Unit = radek.ElkovMJ, // ELKOV_MJ a EMAS_MJ by měly být shodné
                    CategoryId = categoryId
                };
                _db.Materials.Add(material);
                await _db.SaveChangesAsync();
            }
            return material;
        }

        // ---------------------------------------------------------------
        // Upsert (update nebo insert) ceny pro konkrétní dvojici
        // Material + Supplier. Klíčem pro nalezení existujícího záznamu
        // je (SupplierId, SupplierCode) - viz unikátní index v AppDbContext
        // ---------------------------------------------------------------
        private async Task UpsertCenuAsync(int materialId, int supplierId, string kod, string nazev, string mj, decimal cena, string mena)
        {
            var existujici = await _db.MaterialPrices
                .FirstOrDefaultAsync(mp => mp.SupplierId == supplierId && mp.SupplierCode == kod);

            if (existujici != null)
            {
                // Záznam už existuje - jen aktualizujeme cenu a čas aktualizace
                existujici.Price = cena;
                existujici.UpdatedAt = DateTime.Now;
            }
            else
            {
                // Záznam neexistuje - vytvoříme nový
                _db.MaterialPrices.Add(new MaterialPrice
                {
                    MaterialId = materialId,
                    SupplierId = supplierId,
                    SupplierCode = kod,
                    SupplierName = nazev,
                    Unit = mj,
                    Price = cena,
                    Currency = mena
                });
            }

            await _db.SaveChangesAsync();
        }
    }
}