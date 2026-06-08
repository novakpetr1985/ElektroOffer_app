import os

root = r"D:\_GitHub\ElektroOffer_app"
output_dir = os.path.join(root, "_other")
output_file = os.path.join(output_dir, "_SPOJENO-CHOICE.txt")

os.makedirs(output_dir, exist_ok=True)

# 1) Struktura projektu
structure_lines = []
for path, dirs, files in os.walk(root):
    rel = path.replace(root, "")
    structure_lines.append(rel if rel else "/")
    for f in files:
        structure_lines.append(os.path.join(rel, f))

with open(output_file, "w", encoding="utf-8") as out:
    out.write(f"Struktura projektu ({root})\n")
    out.write("------------------------------------------------------------\n")
    for line in structure_lines:
        out.write(line + "\n")
    out.write("------------------------------------------------------------\n\n")

# 2) Jen vybrané soubory
selected_files = [
    r"Data\AppDbContext.cs",
    r"Docs\README.md",
    r"Models\BudgetItems.cs",
    r"Models\CalculationsItems.cs",
    r"Models\CatalogExportData.cs",
    r"Models\Material.cs",
    r"Models\PriceItems.cs",
    r"Models\ProjectData.cs",
    r"Services\ProjectService.cs",
    r"AboutWindow.xaml",
    r"AboutWindow.xaml.cs",
    r"App.xaml",
    r"App.xaml.cs",
    r"AssemblyInfo.cs",
    r"MainWindows.xaml",
    r"MainWindows.xaml.cs"
]

current_folder = ""

with open(output_file, "a", encoding="utf-8") as out:
    for rel in selected_files:
        file_path = os.path.join(root, rel)

        if not os.path.exists(file_path):
            out.write(f"\n===== SOUBOR NENALEZEN: {rel} =====\n")
            continue

        folder = os.path.dirname(rel)

        if folder != current_folder:
            out.write(f"\n===== SLOŽKA: {folder} =====\n")
            current_folder = folder

        out.write(f"--- Soubor: {os.path.basename(rel)}\n")
        out.write(f"--- Cesta: {rel}\n")
        out.write("----------------------------------------\n")

        with open(file_path, "r", encoding="utf-8") as f:
            out.write(f.read())

        out.write("\n\n")

print("Hotovo – soubory byly spojeny do _other/_SPOJENO-CHOICE.txt")
