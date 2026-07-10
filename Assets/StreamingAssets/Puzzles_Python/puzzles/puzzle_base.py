import pygame


DIFICULTAD_TIEMPOS_MS = {
    1: 60000,
    2: 60000,
    3: 45000,
    4: 45000,
    5: 30000,
}

DIFICULTAD_POR_NIVEL = {
    0: 1, 1: 1, 2: 1,
    3: 2, 4: 2, 5: 2,
    6: 3, 7: 3, 8: 3,
    9: 4, 10: 4, 11: 4,
    12: 5, 13: 5, 14: 5,
}


def dificultad_desde_nivel(nivel_idx):
    return DIFICULTAD_POR_NIVEL.get(nivel_idx, 1)


class _SalirPuzzle(Exception):

    def __init__(self, resultado):
        self.resultado = resultado


class BasePuzzle:

    DURACION_EXITO = 1000
    DURACION_FALLO = 1500

    def __init__(
        self,
        pantalla,
        ancho,
        alto,
        fuente_peq,
        dificultad,
        config=None
    ):

        self.pantalla = pantalla

        self.ancho = ancho
        self.alto = alto

        self.dificultad = dificultad
        self.config = config or {}

        self.BASE_ANCHO = 1280
        self.BASE_ALTO = 720

        self.escala_x = ancho / self.BASE_ANCHO
        self.escala_y = alto / self.BASE_ALTO

        margen_x = self.sx(120)
        margen_y = self.sy(100)

        self.area = pygame.Rect(
            margen_x,
            margen_y,
            ancho - margen_x * 2,
            alto - margen_y * 2
        )

        self.estado = "JUGANDO"

        self.tiempo_total = DIFICULTAD_TIEMPOS_MS.get(
            dificultad,
            60000
        )

        self.tiempo_inicio = pygame.time.get_ticks()

        self.tiempo_restante = self.tiempo_total

        try:

            self.fuente_titulo = pygame.font.Font(
                "assets/fonts/PressStart2P-Regular.ttf",
                28
            )

            self.fuente_grande = pygame.font.Font(
                "assets/fonts/PressStart2P-Regular.ttf",
                20
            )

        except:

            self.fuente_titulo = pygame.font.SysFont(
                "Arial",
                32,
                bold=True
            )

            self.fuente_grande = pygame.font.SysFont(
                "Arial",
                20,
                bold=True
            )

        if fuente_peq:

            self.fuente_peq = fuente_peq

        else:

            self.fuente_peq = self.fuente_grande

        self.fuente_etiqueta = self.fuente_peq

        self.sonido_conectar = None
        self.sonido_error = None
        self.sonido_exito = None
        self.sonido_interruptor = None

        self.fondo = pygame.Surface(
            (ancho,alto)
        )

        self.fondo.fill(
            (15,20,35)
        )

        self.exito_inicio = 0
        self.fallo_inicio = 0

        self._construir()

    def sx(self,x):
        return int(
            x * self.escala_x
        )

    def sy(self,y):
        return int(
            y * self.escala_y
        )

    def rect(self,x,y,w,h):
        return pygame.Rect(
            self.sx(x),
            self.sy(y),
            self.sx(w),
            self.sy(h)
        )

    def _construir(self):
        raise NotImplementedError

    def _manejar_evento(self,evento):
        return None

    def _actualizar_subclase(self,dt):
        pass

    def _dibujar_subclase(self):
        raise NotImplementedError

    def _verificar_victoria(self):
        raise NotImplementedError

    def ejecutar(self):
        reloj = pygame.time.Clock()

        try:
            while True:

                for evento in pygame.event.get():

                    if evento.type == pygame.QUIT:
                        return "salir"

                    if evento.type == pygame.KEYDOWN:
                        if evento.key == pygame.K_ESCAPE:
                            return "menu"

                    if self.estado == "JUGANDO":
                        resultado = self._manejar_evento(evento)
                        if resultado:
                            return resultado

                dt = reloj.tick(60)

                self.actualizar(dt)

                self.dibujar()

                pygame.display.flip()

        except _SalirPuzzle as e:
            return e.resultado

    def actualizar(self,dt):

        if self.estado == "JUGANDO":

            tiempo = (
                pygame.time.get_ticks()
                -
                self.tiempo_inicio
            )

            self.tiempo_restante = max(
                0,
                self.tiempo_total - tiempo
            )

            self._actualizar_subclase(dt)

            if self._verificar_victoria():

                self.estado = "EXITO"

                self.exito_inicio = pygame.time.get_ticks()

            elif self.tiempo_restante <= 0:


                self.estado = "FALLO"

                self.fallo_inicio = pygame.time.get_ticks()

        elif self.estado == "EXITO":

            if pygame.time.get_ticks() - self.exito_inicio > self.DURACION_EXITO:

                raise _SalirPuzzle("resuelto")

        elif self.estado == "FALLO":

            if pygame.time.get_ticks() - self.fallo_inicio > self.DURACION_FALLO:

                raise _SalirPuzzle("tiempo_agotado")

    def dibujar(self):

        self.pantalla.blit(
            self.fondo,
            (0,0)
        )

        self._dibujar_subclase()

        segundos = max(
            0,
            self.tiempo_restante // 1000
        )

        texto = self.fuente_grande.render(
            f"TIEMPO {segundos}",
            True,
            (255,255,255)
        )

        self.pantalla.blit(
            texto,
            (20,20)
        )

        if self.estado == "EXITO":

            texto = self.fuente_titulo.render(
                "COMPLETADO",
                True,
                (0,255,100)
            )

            self.pantalla.blit(
                texto,
                texto.get_rect(
                    center=(
                        self.ancho//2,
                        self.alto//2
                    )
                )
            )

        elif self.estado == "FALLO":

            texto = self.fuente_titulo.render(
                "FALLO",
                True,
                (255,80,80)
            )

            self.pantalla.blit(
                texto,
                texto.get_rect(
                    center=(
                        self.ancho//2,
                        self.alto//2
                    )
                )
            )

    def _reproducir(self, sonido):
        if sonido:
            try:
                sonido.play()
            except:
                pass
