import pygame
import sys
import os


from puzzles.puzzle_cables import PuzzleCables


# Configuración de ventana - más compacta, sin bordes, DPI-aware
WIDTH = 900
HEIGHT = 550
WINDOW_FLAGS = pygame.NOFRAME | pygame.SCALED

# Centrar ventana en pantalla
os.environ['SDL_VIDEO_CENTERED'] = '1'


def ejecutar_puzzle(nombre):

    pygame.init()

    pantalla = pygame.display.set_mode(
        (WIDTH, HEIGHT),
        WINDOW_FLAGS
    )

    pygame.display.set_caption(
        "Rack Jacked Man - Puzzle"
    )


    fuente = pygame.font.Font(
        None,
        22
    )


    if nombre == "cables":

        puzzle = PuzzleCables(
            pantalla,
            WIDTH,
            HEIGHT,
            fuente,
            3
        )


        resultado = puzzle.ejecutar()


    else:
        resultado = "error"


    ruta = os.path.join(
        os.path.dirname(__file__),
        "resultado.txt"
    )


    with open(ruta,"w") as f:
        f.write(resultado)


    pygame.quit()



if __name__ == "__main__":

    ejecutar_puzzle("cables")