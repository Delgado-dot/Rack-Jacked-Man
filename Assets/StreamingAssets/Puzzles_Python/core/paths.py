"""
Rutas de assets para puzzles_python.

Todas las rutas son relativas a la carpeta puzzles_python/ (donde está launcher.py).
Esto permite que funcione desde Unity StreamingAssets sin rutas absolutas.
"""

import os


# Directorio base: donde está launcher.py (puzzles_python/)
BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))


def asset_path(*parts: str) -> str:
    """
    Retorna la ruta absoluta a un asset dentro de puzzles_python/assets/.

    Uso:
        asset_path("fonts", "PressStart2P-Regular.ttf")
        -> C:/.../StreamingAssets/puzzles_python/assets/fonts/PressStart2P-Regular.ttf
    """
    return os.path.join(BASE_DIR, "assets", *parts)


def puzzle_path(puzzle_name: str) -> str:
    """
    Retorna la ruta a la carpeta de un puzzle específico.

    Uso:
        puzzle_path("cables")
        -> C:/.../StreamingAssets/puzzles_python/puzzles/cables/
    """
    return os.path.join(BASE_DIR, "puzzles", puzzle_name)


def resultado_path() -> str:
    """
    Retorna la ruta al archivo resultado.txt donde se escribe el resultado.
    """
    return os.path.join(BASE_DIR, "resultado.txt")


def get_base_dir() -> str:
    """Retorna el directorio base (puzzles_python/)."""
    return BASE_DIR