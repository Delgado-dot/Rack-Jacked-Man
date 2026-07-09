"""
MenuPausa - Menú de pausa independiente para puzzles_python.

No depende de ningún sistema externo (menú principal, game.py, states.py, etc.).
Opciones:
- Reanudar: vuelve exactamente al mismo puzzle
- Reiniciar: devuelve "reiniciar" al launcher
- Menú: devuelve "menu" al launcher (Unity maneja volver al menú principal)
- Salir: devuelve "salir" al launcher (Unity cierra la aplicación)
"""

import pygame

from core.paths import asset_path


class MenuPausa:
    """Menú de pausa simple e independiente."""

    # Constantes de clase
    OPCIONES = [
        ("reanudar", "REANUDAR"),
        ("reiniciar", "REINICIAR"),
        ("menu", "MENU PRINCIPAL"),
        ("salir", "SALIR"),
    ]

    COLOR_FONDO = (10, 15, 30, 220)
    COLOR_PANEL = (20, 30, 50, 240)
    COLOR_BORDE = (0, 200, 255)
    COLOR_TEXTO = (220, 230, 240)
    COLOR_SELECCION = (0, 255, 200)
    COLOR_HOVER = (100, 220, 255)

    def __init__(self, pantalla, ancho, alto, config=None):
        self.pantalla = pantalla
        self.ancho = ancho
        self.alto = alto
        self.config = config or {}

        # Capturar fondo del puzzle actual
        self.fondo = pantalla.copy()

        # Fuentes
        try:
            self.fuente_titulo = pygame.font.Font(asset_path("fonts", "PressStart2P-Regular.ttf"), 28)
            self.fuente_opcion = pygame.font.Font(asset_path("fonts", "PressStart2P-Regular.ttf"), 16)
        except Exception:
            self.fuente_titulo = pygame.font.SysFont("Arial", 40, bold=True)
            self.fuente_opcion = pygame.font.SysFont("Arial", 20, bold=True)

        # Layout
        self.panel_w = 400
        self.panel_h = 360
        self.panel_x = (ancho - self.panel_w) // 2
        self.panel_y = (alto - self.panel_h) // 2

        self.opcion_seleccionada = 0
        self.rects_opciones = []
        self._calcular_rects()

        # Sonidos de navegación (opcional)
        self.sonido_navegar = self._cargar_sonido(asset_path("sounds", "menu_navegar.wav"))
        self.sonido_seleccionar = self._cargar_sonido(asset_path("sounds", "menu_seleccionar.wav"))

    def _cargar_sonido(self, ruta):
        try:
            return pygame.mixer.Sound(ruta)
        except Exception:
            return None

    def _reproducir(self, sonido):
        if sonido:
            try:
                sonido.play()
            except Exception:
                pass

    def _calcular_rects(self):
        self.rects_opciones = []
        start_y = self.panel_y + 100
        for i, (key, label) in enumerate(self.OPCIONES):
            render = self.fuente_opcion.render(label, True, self.COLOR_TEXTO)
            rect = render.get_rect(center=(self.ancho // 2, start_y + i * 60))
            self.rects_opciones.append((key, label, rect, render))

    def ejecutar(self) -> str:
        """
        Ejecuta el bucle del menú de pausa.
        Retorna: "reanudar" | "reiniciar" | "menu" | "salir"
        """
        reloj = pygame.time.Clock()

        # Limpiar eventos previos
        pygame.event.clear()

        while True:
            for evento in pygame.event.get():
                if evento.type == pygame.QUIT:
                    return "salir"

                if evento.type == pygame.KEYDOWN:
                    if evento.key == pygame.K_ESCAPE:
                        return "reanudar"
                    elif evento.key in (pygame.K_UP, pygame.K_w):
                        self.opcion_seleccionada = (self.opcion_seleccionada - 1) % len(self.OPCIONES)
                        self._reproducir(self.sonido_navegar)
                    elif evento.key in (pygame.K_DOWN, pygame.K_s):
                        self.opcion_seleccionada = (self.opcion_seleccionada + 1) % len(self.OPCIONES)
                        self._reproducir(self.sonido_navegar)
                    elif evento.key in (pygame.K_RETURN, pygame.K_SPACE):
                        key, _, _, _ = self.OPCIONES[self.opcion_seleccionada]
                        self._reproducir(self.sonido_seleccionar)
                        return key

                if evento.type == pygame.MOUSEMOTION:
                    mouse_pos = pygame.mouse.get_pos()
                    for i, (key, label, rect, _) in enumerate(self.rects_opciones):
                        if rect.collidepoint(mouse_pos):
                            if i != self.opcion_seleccionada:
                                self._reproducir(self.sonido_navegar)
                            self.opcion_seleccionada = i
                            break

                if evento.type == pygame.MOUSEBUTTONDOWN and evento.button == 1:
                    mouse_pos = pygame.mouse.get_pos()
                    for i, (key, label, rect, _) in enumerate(self.rects_opciones):
                        if rect.collidepoint(mouse_pos):
                            self._reproducir(self.sonido_seleccionar)
                            return key

            self._dibujar()
            pygame.display.flip()
            reloj.tick(60)

    def _dibujar(self):
        # Fondo del puzzle congelado
        self.pantalla.blit(self.fondo, (0, 0))

        # Overlay oscuro
        overlay = pygame.Surface((self.ancho, self.alto), pygame.SRCALPHA)
        overlay.fill(self.COLOR_FONDO)
        self.pantalla.blit(overlay, (0, 0))

        # Panel
        panel = pygame.Surface((self.panel_w, self.panel_h), pygame.SRCALPHA)
        panel.fill(self.COLOR_PANEL)
        self.pantalla.blit(panel, (self.panel_x, self.panel_y))

        # Borde del panel
        pygame.draw.rect(
            self.pantalla,
            self.COLOR_BORDE,
            (self.panel_x, self.panel_y, self.panel_w, self.panel_h),
            2,
            border_radius=12
        )

        # Título
        titulo = self.fuente_titulo.render("PAUSA", True, self.COLOR_BORDE)
        titulo_rect = titulo.get_rect(center=(self.ancho // 2, self.panel_y + 50))
        self.pantalla.blit(titulo, titulo_rect)

        # Opciones
        mouse_pos = pygame.mouse.get_pos()
        for i, (key, label, rect, render) in enumerate(self.rects_opciones):
            es_seleccionada = (i == self.opcion_seleccionada)
            hover = rect.collidepoint(mouse_pos)

            if es_seleccionada or hover:
                color = self.COLOR_SELECCION if es_seleccionada else self.COLOR_HOVER
                # Fondo de la opción seleccionada
                bg_rect = rect.inflate(40, 16)
                pygame.draw.rect(self.pantalla, (30, 50, 80, 180), bg_rect, border_radius=8)
                pygame.draw.rect(self.pantalla, color, bg_rect, 2, border_radius=8)
            else:
                color = self.COLOR_TEXTO

            # Re-renderizar con color correcto
            texto = self.fuente_opcion.render(label, True, color)
            texto_rect = texto.get_rect(center=rect.center)
            self.pantalla.blit(texto, texto_rect)

        # Hint teclas
        hint = self.fuente_opcion.render("ESC: Reanudar  |  ↑↓: Navegar  |  ENTER: Seleccionar", True, (100, 130, 160))
        hint_rect = hint.get_rect(center=(self.ancho // 2, self.panel_y + self.panel_h - 30))
        self.pantalla.blit(hint, hint_rect)