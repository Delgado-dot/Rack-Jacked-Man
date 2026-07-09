"""
Effects - Efectos visuales para puzzles_python.
Reemplaza a game.UI.menu_efects.MenuEffects del juego antiguo.
"""

import math
import pygame


class Effects:
    """Efectos visuales simples: partículas, texto con pulso, glow de botones."""

    def __init__(self, ancho: int, alto: int):
        self.ancho = ancho
        self.alto = alto
        self.particulas = []

    # ---------------------------------------------------------------------
    # Partículas simples
    # ---------------------------------------------------------------------
    def agregar_particulas(self, x: float, y: float, color, cantidad: int = 8,
                           vel_min: float = 0.5, vel_max: float = 2.5,
                           vida_min: int = 300, vida_max: int = 700,
                           radio_min: int = 2, radio_max: int = 5):
        """Agrega partículas explosivas en (x, y)."""
        for _ in range(cantidad):
            angulo = math.uniform(0, math.tau)
            vel = math.uniform(vel_min, vel_max)
            self.particulas.append({
                "x": x,
                "y": y,
                "vx": math.cos(angulo) * vel,
                "vy": math.sin(angulo) * vel,
                "vida": math.randint(vida_min, vida_max),
                "vida_max": math.randint(vida_min, vida_max),
                "color": color,
                "radio": math.randint(radio_min, radio_max),
            })

    def actualizar_particulas(self, dt: int):
        """Actualiza partículas (dt en ms)."""
        for p in self.particulas:
            p["x"] += p["vx"] * dt / 16.67
            p["y"] += p["vy"] * dt / 16.67
            p["vida"] -= dt
        self.particulas = [p for p in self.particulas if p["vida"] > 0]

    def dibujar_particulas(self, pantalla):
        """Dibuja todas las partículas activas."""
        for p in self.particulas:
            ratio = max(0, min(1, p["vida"] / p["vida_max"]))
            radio = max(1, int(p["radio"] * ratio))
            alpha = int(220 * ratio)

            # Crear surface con alpha
            surf = pygame.Surface((radio * 2, radio * 2), pygame.SRCALPHA)
            color_con_alpha = (*p["color"][:3], alpha)
            pygame.draw.circle(surf, color_con_alpha, (radio, radio), radio)
            pantalla.blit(surf, (int(p["x"]) - radio, int(p["y"]) - radio))

    # ---------------------------------------------------------------------
    # Texto con efecto de pulso
    # ---------------------------------------------------------------------
    @staticmethod
    def render_texto_pulso(fuente, texto, color, amplitud=1.0, velocidad=0.03):
        """
        Renderiza texto con efecto de pulso (escala oscilante).

        Args:
            fuente: pygame.font.Font
            texto: str
            color: tuple (r, g, b)
            amplitud: factor de escala máximo (1.0 = normal, 1.2 = 20% más grande)
            velocidad: velocidad de oscilación

        Returns:
            pygame.Surface con el texto renderizado
        """
        tiempo = pygame.time.get_ticks() * velocidad
        factor = 1.0 + math.sin(tiempo) * amplitud * 0.1

        render_base = fuente.render(texto, True, color)

        if abs(factor - 1.0) < 0.01:
            return render_base

        nuevo_w = int(render_base.get_width() * factor)
        nuevo_h = int(render_base.get_height() * factor)
        return pygame.transform.smoothscale(render_base, (nuevo_w, nuevo_h))

    # ---------------------------------------------------------------------
    # Glow para botones (hover)
    # ---------------------------------------------------------------------
    @staticmethod
    def dibujar_glow_boton(pantalla, x: int, y: int, w: int, h: int,
                           color=(0, 200, 255), radio=12, intensidad=80):
        """
        Dibuja un glow suave detrás de un botón (efecto hover).

        Args:
            pantalla: surface destino
            x, y, w, h: rect del botón
            color: color del glow (r, g, b)
            radio: radio de borde del botón
            intensidad: alpha máximo del glow
        """
        glow_surf = pygame.Surface((w + 30, h + 30), pygame.SRCALPHA)
        for i in range(8):
            alpha = int(intensidad * (1 - i / 8) * 0.5)
            rect = pygame.Rect(15 - i, 15 - i, w + i * 2, h + i * 2)
            pygame.draw.rect(glow_surf, (*color, alpha), rect, border_radius=radio + i)
        pantalla.blit(glow_surf, (x - 15, y - 15))