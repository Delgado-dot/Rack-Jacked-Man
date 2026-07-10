import pygame
import sys
import os
import importlib


WIDTH = 900
HEIGHT = 550

WINDOW_FLAGS = pygame.NOFRAME | pygame.SCALED

os.environ["SDL_VIDEO_CENTERED"] = "1"


PUZZLES = {
    "cables": "puzzles.puzzle_cables.PuzzleCables",
    "dispatcher": "puzzles.puzzle_dispatcher.PuzzleDispatcher",
    "nave": "puzzles.puzzle_nave.PuzzleNave",
    "trafico": "puzzles.puzzle_trafico.PuzzleTrafico",
    "patchcore": "puzzles.puzzle_patchcore.PuzzlePatchCore",
}


def cargar_puzzle(nombre):
    if nombre not in PUZZLES:
        print("[ERROR] Puzzle no encontrado:", nombre)
        return None

    ruta = PUZZLES[nombre]
    modulo, clase = ruta.rsplit(".", 1)

    try:
        archivo = importlib.import_module(modulo)
        clase_puzzle = getattr(archivo, clase)
        return clase_puzzle
    except Exception as e:
        print(f"[ERROR] Importando {nombre}: {e}")
        return None


def ejecutar_puzzle(nombre):
    pygame.init()

    pantalla = pygame.display.set_mode(
        (WIDTH, HEIGHT),
        WINDOW_FLAGS
    )

    pygame.display.set_caption("Rack Jacked Man - Puzzle")

    fuente = pygame.font.Font(None, 22)

    clase_puzzle = cargar_puzzle(nombre)

    if clase_puzzle is None:
        resultado = "error"
    else:
        try:
            puzzle = clase_puzzle(
                pantalla, WIDTH, HEIGHT, fuente, 3
            )
            resultado = puzzle.ejecutar()
        except Exception as e:
            print("[ERROR EJECUTANDO PUZZLE]")
            print(e)
            resultado = "error"

    ruta_resultado = os.path.join(
        os.path.dirname(__file__),
        "resultado.txt"
    )

    with open(ruta_resultado, "w", encoding="utf-8") as archivo:
        archivo.write(resultado)

    pygame.quit()


if __name__ == "__main__":
    if len(sys.argv) > 1:
        nombre = sys.argv[1]
    else:
        nombre = "cables"

    print(f"[LAUNCHER] Iniciando puzzle: {nombre}")
    ejecutar_puzzle(nombre)
