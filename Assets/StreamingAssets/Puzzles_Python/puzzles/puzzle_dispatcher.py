import random
import pygame

from puzzles.puzzle_base import BasePuzzle, dificultad_desde_nivel
from puzzles.puzzle_cables import PuzzleCables
from puzzles.puzzle_trafico import PuzzleTrafico
from puzzles.puzzle_nave import PuzzleNave
from puzzles.puzzle_patchcore import PuzzlePatchcore


class PuzzleDispatcher:
    REGISTRO = {
        "cables": PuzzleCables,
        "trafico": PuzzleTrafico,
        "nave": PuzzleNave,
        "patchcore": PuzzlePatchcore,
    }

    def __init__(self, pantalla, ancho, alto, fuente_peq, nivel_idx, semilla=None, config=None):
        self.dificultad = dificultad_desde_nivel(nivel_idx)
        self.config = config or {}

        if semilla is None:
            semilla = pygame.time.get_ticks()

        rng = random.Random(semilla)
        tipo = rng.choice(list(self.REGISTRO.keys()))
        cls = self.REGISTRO[tipo]

        self._instancia = cls(
            pantalla,
            ancho,
            alto,
            fuente_peq,
            self.dificultad,
            config=self.config
        )

    def ejecutar(self):
        self._iniciar_musica_puzzle()

        try:
            resultado = self._instancia.ejecutar()
            return resultado
        finally:
            self._detener_musica_puzzle()

    def _iniciar_musica_puzzle(self):
        try:
            pygame.mixer.stop()
        except:
            pass

        try:
            pygame.mixer.music.stop()
        except:
            pass

        try:
            if hasattr(pygame.mixer.music, "unload"):
                pygame.mixer.music.unload()
        except:
            pass

        try:
            pygame.mixer.music.load(self.config.get("musica_puzzle", "assets/sounds/puzzle_music.ogg"))
            pygame.mixer.music.set_volume(self.config.get("vol_musica", 0.5))
            pygame.mixer.music.play(-1)
            print("[AUDIO PUZZLE] Música puzzle iniciada")
        except Exception as error:
            print("[ERROR AUDIO PUZZLE]", error)

    def _detener_musica_puzzle(self):
        try:
            pygame.mixer.music.stop()
        except:
            pass

        try:
            pygame.mixer.stop()
        except:
            pass

        try:
            if hasattr(pygame.mixer.music, "unload"):
                pygame.mixer.music.unload()
        except:
            pass

        print("[AUDIO PUZZLE] Música puzzle detenida")