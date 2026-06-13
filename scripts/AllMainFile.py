import os

# ROOT = hlavní složka projektu, kterou chceme projít
ROOT = r"D:\_GitHub\ElektroOffer_app"

# OUTPUT = kam uložíme výstup (bez přípony)
OUTPUT = os.path.join(ROOT, "scripts/scripts-output", "AllMainFile.txt")

# Složky, které nechceme zahrnout do stromu
IGNORE_DIRS = {"bin", "obj", ".git", ".vs", "__pycache__", "scripts", "data"}


def should_ignore(path):
    """
    Vrací True, pokud cesta obsahuje složku,
    kterou nechceme zahrnout do výpisu.
    """
    parts = path.split(os.sep)
    return any(p in IGNORE_DIRS for p in parts)


def write_tree(root, out_file):
    """
    Projde celý projekt a zapíše stromovou strukturu do souboru.
    """
    for folder, dirs, files in os.walk(root):

        # Přeskočíme ignorované složky
        if should_ignore(folder):
            continue

        # Relativní cesta vůči ROOT
        rel_folder = os.path.relpath(folder, root)

        # Hlavní složka = "."
        if rel_folder == ".":
            rel_folder = root

        # Zapíšeme název složky
        out_file.write(f"\n📁 {rel_folder}\n")

        # Zapíšeme soubory ve složce
        for f in files:
            full = os.path.join(folder, f)

            if should_ignore(full):
                continue

            out_file.write(f"   └── {f}\n")


def main():
    """
    Hlavní funkce skriptu.
    """
    # Vytvoří složku data, pokud neexistuje
    os.makedirs(os.path.dirname(OUTPUT), exist_ok=True)

    # Otevřeme výstupní soubor
    with open(OUTPUT, "w", encoding="utf-8") as out:
        out.write("=== STROMOVÁ STRUKTURA PROJEKTU ===\n")

        # Zapíšeme strom
        write_tree(ROOT, out)

    print(f"Hotovo! Stromová struktura je zde:\n{OUTPUT}")


if __name__ == "__main__":
    main()
