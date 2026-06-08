import os

root = r"D:\_GitHub\ElektroOffer_app"
output_dir = os.path.join(root, "_other")
output_file = os.path.join(output_dir, "_SPOJENO-ALL.txt")

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

    # 2) Soubory podle složek
    all_files = []
    for path, dirs, files in os.walk(root):
        for f in files:
            if f.endswith((".cs", ".xaml", ".md")) and "_other" not in path:
                all_files.append(os.path.join(path, f))

    all_files.sort()

    current_folder = ""

    for file in all_files:
        rel = file.replace(root, "")
        folder = os.path.dirname(rel)

        if folder != current_folder:
            out.write(f"\n===== SLOŽKA: {folder} =====\n")
            current_folder = folder

        out.write(f"--- Soubor: {os.path.basename(file)}\n")
        out.write(f"--- Cesta: {rel}\n")
        out.write("----------------------------------------\n")

        with open(file, "r", encoding="utf-8") as f:
            out.write(f.read())

        out.write("\n\n")

print("Hotovo – soubory byly spojeny do _other/_SPOJENO-ALL.txt")
