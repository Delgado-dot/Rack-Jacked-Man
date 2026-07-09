"""
BasePuzzle: clase base compartida por todos los puzles.

Corregido:
- ESC abre pausa dentro de cualquier puzzle.
- Reanudar vuelve al mismo puzzle sin dejar pegado el menú.
- El cronómetro se congela mientras estás en pausa.
- La música del puzzle se pausa y vuelve al reanudar.
- Reiniciar / menú / salir se devuelven bien al main.
"""

import math
import pygame




DIFICULTAD_TIEMPOS_MS = {
    1: 60_000,
    2: 60_000,
    3: 45_000,
    4: 45_000,
    5: 30_000,
}


def dificultad_desde_nivel(nivel_idx: int) -> int:
    return min(5, max(1, nivel_idx + 1))


class _SalirPuzzle(Exception):
    def __init__(self, resultado):
        self.resultado = resultado


class BasePuzzle:
    DURACION_ENTRADA = 350
    DURACION_EXITO = 800
    DURACION_TIMEOUT_OVERLAY = 1500

    HEADER_H = 80
    FOOTER_H = 100
    PAD = 24

    def __init__(self, pantalla, ancho, alto, fuente_peq, dificultad, config=None):
        self.pantalla = pantalla
        self.ancho = ancho
        self.alto = alto
        self.dificultad = dificultad
        self.fuente_peq = fuente_peq
        self.config = config or {}

        self.effects = MenuEffects(ancho, alto)
        self.fondo_congelado = pantalla.copy()

        try:
            self.fuente_titulo = pygame.font.Font("assets/fonts/PressStart2P-Regular.ttf", 25)
            self.fuente_etiqueta = pygame.font.Font("assets/fonts/PressStart2P-Regular.ttf", 11)
            self.fuente_grande = pygame.font.Font("assets/fonts/PressStart2P-Regular.ttf", 22)
        except Exception:
            self.fuente_titulo = pygame.font.SysFont("Arial", 40, bold=True)
            self.fuente_etiqueta = pygame.font.SysFont("Arial", 16, bold=True)
            self.fuente_grande = pygame.font.SysFont("Arial", 22, bold=True)

        if fuente_peq is not None:
            self.fuente_peq = fuente_peq
        else:
            self.fuente_peq = self.fuente_etiqueta

        self.sonido_conectar = self._cargar_sonido("assets/sounds/cable_conectar.wav")
        self.sonido_interruptor = self._cargar_sonido("assets/sounds/interruptor.wav")
        self.sonido_error = self._cargar_sonido("assets/sounds/error.wav")
        self.sonido_exito = self._cargar_sonido("assets/sounds/exito.wav")
        self.sonido_tick = self._cargar_sonido("assets/sounds/tick.wav")

        self.duracion_total_ms = DIFICULTAD_TIEMPOS_MS.get(dificultad, 60_000)
        self.tiempo_inicio = pygame.time.get_ticks()
        self.tiempo_restante_ms = self.duracion_total_ms
        self._ultimo_segundo_marcado = -1

        self.estado = "JUGANDO"
        self.exito_inicio = 0
        self.timeout_inicio = 0
        self.tiempo_inicio_ms = pygame.time.get_ticks()

        self.panel_w = int(ancho * 0.86)
        self.panel_h = int(alto * 0.82)
        self.panel_x = (ancho - self.panel_w) // 2
        self.panel_y = (alto - self.panel_h) // 2

        header_bottom = self.panel_y + self.HEADER_H + 12
        footer_top = self.panel_y + self.panel_h - self.FOOTER_H - 12

        self.area = pygame.Rect(
            self.panel_x + self.PAD,
            header_bottom,
            self.panel_w - 2 * self.PAD,
            footer_top - header_bottom,
        )

        self.boton_salir_rect = self._rect_boton("PAUSA (ESC)", esquina="inf_der")
        self.boton_validar_rect = (
            self._rect_boton("VALIDAR", esquina="inf_cent_der")
            if self._tiene_boton_validar()
            else None
        )

        self.toast_texto = ""
        self.toast_color = (255, 120, 120)
        self.toast_inicio = 0
        self.toast_duracion = 1500

        self._construir()

    def _tiene_boton_validar(self) -> bool:
        return False

    def _construir(self):
        raise NotImplementedError

    def _manejar_evento(self, evento):
        return None

    def _actualizar_subclase(self, dt):
        pass

    def _dibujar_subclase(self):
        raise NotImplementedError

    def _verificar_victoria(self) -> bool:
        raise NotImplementedError

    def ejecutar(self) -> str:
        reloj = pygame.time.Clock()

        try:
            while True:
                for evento in pygame.event.get():
                    if evento.type == pygame.QUIT:
                        return "salir"

                    if evento.type == pygame.KEYDOWN and evento.key == pygame.K_ESCAPE:
                        resultado_pausa = self._abrir_pausa()

                        if resultado_pausa == "reanudar":
                            continue

                        return resultado_pausa

                    if self.estado == "JUGANDO":
                        if (
                            evento.type == pygame.MOUSEBUTTONDOWN
                            and evento.button == 1
                            and self.boton_salir_rect.collidepoint(evento.pos)
                        ):
                            resultado_pausa = self._abrir_pausa()

                            if resultado_pausa == "reanudar":
                                continue

                            return resultado_pausa

                        if (
                            self.boton_validar_rect is not None
                            and evento.type == pygame.MOUSEBUTTONDOWN
                            and evento.button == 1
                            and self.boton_validar_rect.collidepoint(evento.pos)
                        ):
                            if self._verificar_victoria():
                                self._disparar_exito()
                            else:
                                self._mostrar_toast("Asignación incompleta o incorrecta", (255, 120, 120))
                            continue

                        res = self._manejar_evento(evento)

                        if res is not None:
                            return res

                dt = reloj.tick(60)
                self._actualizar(dt)
                self._dibujar()
                pygame.display.flip()

        except _SalirPuzzle as e:
            return e.resultado

    def _abrir_pausa(self):
        """
        Pausa limpia para puzzles:
        - Dibuja el puzzle antes de abrir pausa.
        - Pausa la música del puzzle.
        - Congela el cronómetro.
        - Al reanudar, limpia eventos para que ESC no se dispare dos veces.
        """
        tiempo_restante_antes_pausa = self.tiempo_restante_ms

        try:
            self._dibujar()
            pygame.display.flip()
        except Exception:
            pass

        try:
            pygame.event.clear()
        except Exception:
            pass

        try:
            pygame.mixer.music.pause()
        except Exception:
            pass

        pausa = MenuPausa(self.pantalla, self.ancho, self.alto, self.config)
        resultado = pausa.ejecutar()

        try:
            pygame.event.clear()
        except Exception:
            pass

        if resultado == "reanudar":
            try:
                pygame.mixer.music.unpause()
            except Exception:
                pass

            self.tiempo_restante_ms = tiempo_restante_antes_pausa
            self.tiempo_inicio = pygame.time.get_ticks() - (self.duracion_total_ms - self.tiempo_restante_ms)

            try:
                self._dibujar()
                pygame.display.flip()
            except Exception:
                pass

            return "reanudar"

        try:
            pygame.mixer.music.stop()
        except Exception:
            pass

        if resultado in ("reiniciar", "menu", "salir"):
            return resultado

        return "reanudar"

    def _disparar_exito(self):
        if self.estado != "JUGANDO":
            return

        self.estado = "EXITO"
        self.exito_inicio = pygame.time.get_ticks()
        self._reproducir(self.sonido_exito)

    def _disparar_timeout(self):
        if self.estado != "JUGANDO":
            return

        self.estado = "TIMEOUT"
        self.timeout_inicio = pygame.time.get_ticks()
        self._reproducir(self.sonido_error)

    def _actualizar(self, dt):
        if self.pausado:
            return
        if self.estado == "JUGANDO":
            self.tiempo_restante_ms = max(
                0,
                self.duracion_total_ms - (pygame.time.get_ticks() - self.tiempo_inicio)
            )

            segundos_actuales = int(math.ceil(self.tiempo_restante_ms / 1000))

            if segundos_actuales != self._ultimo_segundo_marcado and self.tiempo_restante_ms > 0:
                self._ultimo_segundo_marcado = segundos_actuales

                if 0 < segundos_actuales <= 5:
                    self._reproducir(self.sonido_tick)

            self._actualizar_subclase(dt)

            if self._verificar_victoria():
                self._disparar_exito()
            elif self.tiempo_restante_ms <= 0:
                self._disparar_timeout()

        elif self.estado == "EXITO":
            if pygame.time.get_ticks() - self.exito_inicio >= self.DURACION_EXITO:
                raise _SalirPuzzle("resuelto")

        elif self.estado == "TIMEOUT":
            if pygame.time.get_ticks() - self.timeout_inicio >= self.DURACION_TIMEOUT_OVERLAY:
                raise _SalirPuzzle("tiempo_agotado")
            if self.pausado:
                self._dibujar_pausa()
    def _dibujar_pausa(self):

        overlay = pygame.Surface(
            (self.ancho,self.alto),
            pygame.SRCALPHA
        )

        overlay.fill(
            (0,0,0,160)
        )

        self.pantalla.blit(
            overlay,
            (0,0)
        )


        texto = self.fuente_titulo.render(
            "PAUSA",
            True,
            (255,255,255)
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
    def _mostrar_toast(self, texto, color=(255, 120, 120)):
        self.toast_texto = texto
        self.toast_color = color
        self.toast_inicio = pygame.time.get_ticks()

    def _dibujar(self):
        progreso = self._progreso_entrada()
        eased = 1 - (1 - progreso) ** 3

        self.pantalla.blit(self.fondo_congelado, (0, 0))

        overlay = pygame.Surface((self.ancho, self.alto), pygame.SRCALPHA)
        overlay.fill((0, 0, 0, int(180 * eased)))
        self.pantalla.blit(overlay, (0, 0))

        self.effects.dibujar_particulas(self.pantalla)

        panel_w = self.panel_w
        panel_h = self.panel_h
        panel_x = (self.ancho - panel_w) // 2
        panel_y = (self.alto - panel_h) // 2

        base = pygame.Surface((panel_w, panel_h), pygame.SRCALPHA)
        base.fill((15, 22, 38, 220))
        self.pantalla.blit(base, (panel_x, panel_y))

        pygame.draw.rect(
            self.pantalla,
            (0, 220, 255),
            (panel_x, panel_y, panel_w, panel_h),
            2,
            border_radius=18
        )

        pygame.draw.rect(
            self.pantalla,
            (80, 160, 220, 80),
            (panel_x + 4, panel_y + 4, panel_w - 8, panel_h - 8),
            1,
            border_radius=14
        )

        self._dibujar_header(panel_x, panel_y, panel_w, eased)

        linea_y = panel_y + self.HEADER_H
        ancho_linea = int((panel_w - 100) * eased)
        linea_x = panel_x + (panel_w - ancho_linea) // 2

        for i in range(3):
            color = (0, 220, 255, max(40, 220 - i * 80))
            pygame.draw.line(
                self.pantalla,
                color,
                (linea_x, linea_y + i),
                (linea_x + ancho_linea, linea_y + i),
                1
            )

        area_x = panel_x + self.PAD
        area_y = panel_y + self.HEADER_H + 12
        area_w = panel_w - 2 * self.PAD
        area_h = panel_h - self.HEADER_H - self.FOOTER_H - 24

        self.area = pygame.Rect(area_x, area_y, area_w, area_h)

        self.boton_salir_rect = self._rect_boton_footer(
            "PAUSA (ESC)",
            panel_x,
            panel_y,
            panel_w,
            panel_h,
            "der"
        )

        if self._tiene_boton_validar():
            self.boton_validar_rect = self._rect_boton_footer(
                "VALIDAR",
                panel_x,
                panel_y,
                panel_w,
                panel_h,
                "der_izq"
            )
        else:
            self.boton_validar_rect = None

        self._dibujar_subclase()
        self._dibujar_footer(panel_x, panel_y, panel_w, panel_h, eased)
        self._dibujar_toast()

        if self.estado == "EXITO":
            self._dibujar_animacion_exito()
        elif self.estado == "TIMEOUT":
            self._dibujar_overlay_timeout()

    def _dibujar_header(self, px, py, pw, eased):
        titulo_color = (0, 240, 255)

        if self.estado == "EXITO":
            titulo_color = (120, 255, 180)
        elif self.estado == "TIMEOUT":
            titulo_color = (255, 120, 120)

        titulo = self.effects.render_texto_pulso(
            self.fuente_titulo,
            self.TITULO,
            titulo_color,
            1,
            0.03
        )

        titulo.set_alpha(int(255 * eased))

        self.pantalla.blit(
            titulo,
            (px + self.PAD, py + (self.HEADER_H - titulo.get_height()) // 2)
        )

        estrellas_y = py + self.HEADER_H // 2
        estrellas_total_x = 5 * 24
        estrellas_x = px + (pw - estrellas_total_x) // 2

        for i in range(5):
            cx = estrellas_x + i * 24 + 8
            puntos = self._puntos_estrella(cx, estrellas_y, 9, 5)

            if i < self.dificultad:
                pygame.draw.polygon(self.pantalla, (255, 220, 80), puntos)
                pygame.draw.polygon(self.pantalla, (180, 130, 30), puntos, 1)
            else:
                pygame.draw.polygon(self.pantalla, (100, 100, 120), puntos, 1)

        segundos_total = max(0, int(math.ceil(self.tiempo_restante_ms / 1000)))
        minutos = segundos_total // 60
        segundos = segundos_total % 60

        timer_texto = f"{minutos:02}:{segundos:02}"

        ratio = self.tiempo_restante_ms / self.duracion_total_ms if self.duracion_total_ms > 0 else 0

        if ratio > 0.5:
            color_timer = (0, 240, 255)
        elif ratio > 0.25:
            color_timer = (255, 220, 80)
        else:
            color_timer = (255, 100, 100)

        if segundos_total <= 10 and self.estado == "JUGANDO":
            pulso = (math.sin(pygame.time.get_ticks() * 0.012) + 1) / 2
            factor = 1 + pulso * 0.15
            render = self.fuente_grande.render(timer_texto, True, color_timer)
            nuevo = pygame.transform.smoothscale(
                render,
                (
                    int(render.get_width() * factor),
                    int(render.get_height() * factor)
                )
            )
        else:
            nuevo = self.fuente_grande.render(timer_texto, True, color_timer)

        nuevo.set_alpha(int(255 * eased))

        self.pantalla.blit(
            nuevo,
            (
                px + pw - self.PAD - nuevo.get_width(),
                py + (self.HEADER_H - nuevo.get_height()) // 2
            )
        )

    def _dibujar_footer(self, px, py, pw, ph, eased):
        hint = self._hint_text()
        hint_render = self.fuente_etiqueta.render(hint, True, (180, 200, 220))
        hint_render.set_alpha(int(220 * eased))

        hint_x = px + self.PAD
        hint_y = py + ph - self.FOOTER_H + 18

        self.pantalla.blit(hint_render, (hint_x, hint_y))

        self._dibujar_boton(
            self.boton_salir_rect,
            "PAUSA (ESC)",
            color_pildora=(70, 25, 35),
            color_borde=(220, 100, 120),
            color_texto=(255, 200, 210),
            eased=eased
        )

        if self.boton_validar_rect is not None:
            self._dibujar_boton(
                self.boton_validar_rect,
                "VALIDAR",
                color_pildora=(20, 60, 30),
                color_borde=(120, 240, 160),
                color_texto=(200, 255, 220),
                eased=eased
            )

        barra_x = px + self.PAD
        barra_y = py + ph - 26
        barra_w = pw - 2 * self.PAD
        barra_h = 12

        ratio = self.tiempo_restante_ms / self.duracion_total_ms if self.duracion_total_ms > 0 else 0

        if ratio > 0.5:
            color_barra = (0, 220, 255)
        elif ratio > 0.25:
            color_barra = (255, 220, 80)
        else:
            color_barra = (255, 100, 100)

        pygame.draw.rect(
            self.pantalla,
            (30, 35, 50),
            (barra_x, barra_y, barra_w, barra_h),
            border_radius=6
        )

        pygame.draw.rect(
            self.pantalla,
            (90, 100, 120),
            (barra_x, barra_y, barra_w, barra_h),
            1,
            border_radius=6
        )

        ancho_relleno = int(barra_w * ratio)

        if ancho_relleno > 0:
            pygame.draw.rect(
                self.pantalla,
                color_barra,
                (barra_x, barra_y, ancho_relleno, barra_h),
                border_radius=6
            )

    def _dibujar_boton(self, rect, texto, color_pildora, color_borde, color_texto, eased):
        mouse_pos = pygame.mouse.get_pos()
        hover = rect.collidepoint(mouse_pos)

        if hover:
            self.effects.dibujar_glow_boton(
                self.pantalla,
                rect.x,
                rect.y,
                rect.width,
                rect.height
            )

        pygame.draw.rect(self.pantalla, color_pildora, rect, border_radius=10)
        pygame.draw.rect(self.pantalla, color_borde, rect, 2, border_radius=10)

        render = self.fuente_etiqueta.render(texto, True, color_texto)
        render.set_alpha(int(255 * eased))

        self.pantalla.blit(render, render.get_rect(center=rect.center))

    def _dibujar_toast(self):
        if not self.toast_texto:
            return

        if pygame.time.get_ticks() - self.toast_inicio > self.toast_duracion:
            self.toast_texto = ""
            return

        texto = self.fuente_etiqueta.render(self.toast_texto, True, self.toast_color)
        padding = 14
        w = texto.get_width() + padding * 2
        h = texto.get_height() + padding
        x = (self.ancho - w) // 2
        y = self.panel_y - h - 12

        fondo = pygame.Surface((w, h), pygame.SRCALPHA)
        fondo.fill((30, 15, 20, 230))

        self.pantalla.blit(fondo, (x, y))
        pygame.draw.rect(self.pantalla, self.toast_color, (x, y, w, h), 2, border_radius=8)
        self.pantalla.blit(texto, (x + padding, y + padding // 2))

    def _dibujar_animacion_exito(self):
        t = pygame.time.get_ticks() - self.exito_inicio
        progreso = min(1.0, t / self.DURACION_EXITO)

        alpha = int(180 * (1.0 - progreso))
        flash = pygame.Surface((self.ancho, self.alto), pygame.SRCALPHA)
        flash.fill((0, 220, 255, alpha))
        self.pantalla.blit(flash, (0, 0))

        ok = self.fuente_titulo.render("CONEXION EXITOSA", True, (255, 255, 255))
        ok.set_alpha(int(255 * (1.0 - progreso * 0.4)))

        escala = 1.0 + progreso * 0.2
        nuevo = pygame.transform.smoothscale(
            ok,
            (
                int(ok.get_width() * escala),
                int(ok.get_height() * escala)
            )
        )

        self.pantalla.blit(
            nuevo,
            nuevo.get_rect(center=(self.ancho // 2, self.alto // 2))
        )

    def _dibujar_overlay_timeout(self):
        t = pygame.time.get_ticks() - self.timeout_inicio
        progreso = min(1.0, t / self.DURACION_TIMEOUT_OVERLAY)

        alpha = int(200 * (1.0 - progreso * 0.5))
        fondo = pygame.Surface((self.ancho, self.alto), pygame.SRCALPHA)
        fondo.fill((60, 10, 10, alpha))
        self.pantalla.blit(fondo, (0, 0))

        texto = self.fuente_titulo.render("TIEMPO AGOTADO", True, (255, 100, 100))
        texto.set_alpha(int(255 * (1.0 - progreso * 0.3)))

        self.pantalla.blit(
            texto,
            texto.get_rect(center=(self.ancho // 2, self.alto // 2))
        )

    def _progreso_entrada(self):
        return min(
            1.0,
            (pygame.time.get_ticks() - self.tiempo_inicio_ms) / self.DURACION_ENTRADA
        )

    def _hint_text(self):
        return getattr(self, "HINT", "")

    def _rect_boton(self, texto, esquina="inf_der"):
        w = 180
        h = 44

        if esquina == "inf_der":
            x = self.panel_x + self.panel_w - w - self.PAD - 8
        elif esquina == "inf_cent_der":
            x = self.panel_x + self.panel_w - 2 * w - self.PAD - 24
        else:
            x = self.panel_x + self.PAD

        y = self.panel_y + self.panel_h - h - 38

        return pygame.Rect(x, y, w, h)

    def _rect_boton_footer(self, texto, panel_x, panel_y, panel_w, panel_h, pos):
        w = 180
        h = 44
        y = panel_y + panel_h - h - 38
        gap = 12

        if pos == "der":
            return pygame.Rect(
                panel_x + panel_w - w - self.PAD - 8,
                y,
                w,
                h
            )

        if pos == "der_izq":
            return pygame.Rect(
                panel_x + panel_w - 2 * w - self.PAD - gap - 24,
                y,
                w,
                h
            )

        return pygame.Rect(panel_x + self.PAD, y, w, h)

    @staticmethod
    def _puntos_estrella(cx, cy, radio_externo, puntas=5):
        puntos = []

        for i in range(puntas * 2):
            angulo = -math.pi / 2 + i * math.pi / puntas
            r = radio_externo if i % 2 == 0 else radio_externo * 0.45
            x = cx + math.cos(angulo) * r
            y = cy + math.sin(angulo) * r
            puntos.append((x, y))

        return puntos

    @staticmethod
    def _cargar_sonido(ruta):
        try:
            return pygame.mixer.Sound(ruta)
        except Exception:
            return None

    @staticmethod
    def _reproducir(sonido):
        if sonido is None:
            return

        try:
            sonido.play()
        except Exception:
            pass
