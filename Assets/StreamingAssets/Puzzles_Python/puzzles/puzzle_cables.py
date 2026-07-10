"""
PuzzleCables — patch panel con etiquetas de protocolos de red.
"""

import math
import random
import pygame

from puzzles.puzzle_base import BasePuzzle


PROTOCOLOS = [
    {"nombre": "TCP",    "color": (255, 100, 100), "hex": "0x06"},
    {"nombre": "UDP",    "color": (100, 180, 255), "hex": "0x11"},
    {"nombre": "HTTP",   "color": (255, 200, 100), "hex": "0x80"},
    {"nombre": "DNS",    "color": (180, 130, 255), "hex": "0x35"},
    {"nombre": "FTP",    "color": (130, 220, 180), "hex": "0x14"},
    {"nombre": "SSH",    "color": (255, 150, 200), "hex": "0x16"},
    {"nombre": "ICMP",   "color": (200, 200, 100), "hex": "0x01"},
    {"nombre": "ARP",    "color": (150, 220, 255), "hex": "0x08"},
    {"nombre": "SMTP",   "color": (220, 130, 100), "hex": "0x19"},
]


class PuzzleCables(BasePuzzle):
    TITULO = "PATCH PANEL"
    HINT = "Click en 2 puertos del mismo protocolo para conectar  |  ESC pausa"
    DURACION_ERROR = 320
    RADIO_HIT = 28

    def _construir(self):
        if self.dificultad <= 2:
            n = 3
        elif self.dificultad <= 4:
            n = 4
        else:
            n = 5

        rng = random.Random(pygame.time.get_ticks())

        self.cables = []
        elegidos = rng.sample(PROTOCOLOS, n)

        for i, prot in enumerate(elegidos):
            self.cables.append({
                "id": i,
                "protocolo": prot["nombre"],
                "color": prot["color"],
                "hex": prot["hex"],
                "puerto_izq": None,
                "puerto_der": None,
                "conectado": False,
            })

        self.puertos = []

        for cab in self.cables:
            self.puertos.append({
                "id": len(self.puertos),
                "cable_id": cab["id"],
                "protocolo": cab["protocolo"],
                "color": cab["color"],
                "hex": cab["hex"],
                "lado": "L",
                "x": 0,
                "y": 0,
                "conectado_a": None,
            })

            self.puertos.append({
                "id": len(self.puertos),
                "cable_id": cab["id"],
                "protocolo": cab["protocolo"],
                "color": cab["color"],
                "hex": cab["hex"],
                "lado": "R",
                "x": 0,
                "y": 0,
                "conectado_a": None,
            })

        ids_izq = [p["id"] for p in self.puertos if p["lado"] == "L"]
        ids_der_shuffled = [p["id"] for p in self.puertos if p["lado"] == "R"]
        rng.shuffle(ids_der_shuffled)

        n = len(self.cables)

        col_izq_x = self.area.x + 110
        col_der_x = self.area.x + self.area.width - 110

        pad_y = 40
        paso = (self.area.height - pad_y * 2) // max(1, n)

        for i, pid in enumerate(ids_izq):
            p = self.puertos[pid]
            p["x"] = col_izq_x
            p["y"] = self.area.y + pad_y + paso * i + paso // 2

        for i, pid in enumerate(ids_der_shuffled):
            p = self.puertos[pid]
            p["x"] = col_der_x
            p["y"] = self.area.y + pad_y + paso * i + paso // 2

        for cab in self.cables:
            cab["puerto_izq"] = [
                p["id"] for p in self.puertos
                if p["lado"] == "L"
            ][cab["id"]]

            cab["puerto_der"] = [
                p["id"] for p in self.puertos
                if p["lado"] == "R" and p["cable_id"] == cab["id"]
            ][0]

        self.puerto_seleccionado = None
        self.error_a = None
        self.error_b = None
        self.error_tiempo = 0
        self.pausado = False

    def _verificar_victoria(self):
        return all(cab["conectado"] for cab in self.cables)

    def _manejar_evento(self, evento):
        if evento.type == pygame.KEYDOWN:
            if evento.key == pygame.K_ESCAPE:
                self.pausado = not self.pausado
                return None

        if self.pausado:
            return None

        if evento.type != pygame.MOUSEBUTTONDOWN:
            return None

        if evento.button != 1:
            return None

        mouse_pos = pygame.mouse.get_pos()

        for p in self.puertos:
            if p["conectado_a"] is not None:
                continue

            dx = mouse_pos[0] - p["x"]
            dy = mouse_pos[1] - p["y"]

            if dx * dx + dy * dy <= self.RADIO_HIT ** 2:
                self._click_puerto(p["id"])
                break

        return None

    def _click_puerto(self, pid):
        p = self.puertos[pid]

        if self.puerto_seleccionado is None:
            self.puerto_seleccionado = pid
            return

        if self.puerto_seleccionado == pid:
            self.puerto_seleccionado = None
            return

        a = self.puertos[self.puerto_seleccionado]
        b = p

        if a["cable_id"] == b["cable_id"] and a["lado"] != b["lado"]:
            a["conectado_a"] = b["id"]
            b["conectado_a"] = a["id"]
            self.cables[a["cable_id"]]["conectado"] = True
            self.puerto_seleccionado = None
            self._reproducir(self.sonido_conectar)
        else:
            self.error_a = a["id"]
            self.error_b = b["id"]
            self.error_tiempo = pygame.time.get_ticks()
            self.puerto_seleccionado = None
            self._reproducir(self.sonido_error)

    def _actualizar_subclase(self, dt):
        if self.error_a is not None:
            if pygame.time.get_ticks() - self.error_tiempo > self.DURACION_ERROR:
                self.error_a = None
                self.error_b = None

    def _dibujar_subclase(self):
        for cab in self.cables:
            if not cab["conectado"]:
                continue

            a = self.puertos[cab["puerto_izq"]]
            b = self.puertos[cab["puerto_der"]]
            color = cab["color"]

            halo = pygame.Surface((self.ancho, self.alto), pygame.SRCALPHA)
            pygame.draw.line(
                halo,
                (*color, 100),
                (a["x"], a["y"]),
                (b["x"], b["y"]),
                14
            )
            self.pantalla.blit(halo, (0, 0))

            pygame.draw.line(
                self.pantalla,
                (
                    max(0, color[0] - 60),
                    max(0, color[1] - 60),
                    max(0, color[2] - 60)
                ),
                (a["x"], a["y"]),
                (b["x"], b["y"]),
                8
            )

            pygame.draw.line(
                self.pantalla,
                color,
                (a["x"], a["y"]),
                (b["x"], b["y"]),
                4
            )

            pygame.draw.line(
                self.pantalla,
                (255, 255, 255),
                (a["x"], a["y"]),
                (b["x"], b["y"]),
                1
            )

        if self.puerto_seleccionado is not None:
            sp = self.puertos[self.puerto_seleccionado]
            mx, my = pygame.mouse.get_pos()
            color = sp["color"]

            halo = pygame.Surface((self.ancho, self.alto), pygame.SRCALPHA)
            pygame.draw.line(
                halo,
                (*color, 80),
                (sp["x"], sp["y"]),
                (mx, my),
                10
            )
            self.pantalla.blit(halo, (0, 0))

            pygame.draw.line(
                self.pantalla,
                (
                    max(0, color[0] - 40),
                    max(0, color[1] - 40),
                    max(0, color[2] - 40)
                ),
                (sp["x"], sp["y"]),
                (mx, my),
                6
            )

            pygame.draw.line(
                self.pantalla,
                color,
                (sp["x"], sp["y"]),
                (mx, my),
                3
            )

            pygame.draw.line(
                self.pantalla,
                (220, 230, 240),
                (sp["x"], sp["y"]),
                (mx, my),
                1
            )

        for p in self.puertos:
            self._dibujar_puerto(p)

    def _dibujar_puerto(self, p):
        x, y = p["x"], p["y"]
        color = p["color"]
        radio = 24
        ahora = pygame.time.get_ticks()

        en_error = (p["id"] == self.error_a or p["id"] == self.error_b)

        if en_error:
            pulso = (math.sin(ahora * 0.04) + 1) / 2
            radio_efectivo = radio + int(pulso * 5)

            surf = pygame.Surface(
                (radio_efectivo * 2 + 6, radio_efectivo * 2 + 6),
                pygame.SRCALPHA
            )

            pygame.draw.circle(
                surf,
                (255, 60, 60),
                (radio_efectivo + 3, radio_efectivo + 3),
                radio_efectivo
            )

            pygame.draw.circle(
                surf,
                (255, 255, 255),
                (radio_efectivo + 3, radio_efectivo + 3),
                radio_efectivo,
                3
            )

            self.pantalla.blit(
                surf,
                (x - radio_efectivo - 3, y - radio_efectivo - 3)
            )
            return

        if self.puerto_seleccionado == p["id"]:
            pulso = (math.sin(ahora * 0.008) + 1) / 2
            halo = pygame.Surface((radio * 5, radio * 5), pygame.SRCALPHA)

            pygame.draw.circle(
                halo,
                (*color, 120 + int(pulso * 80)),
                (int(radio * 2.5), int(radio * 2.5)),
                radio + 8 + int(pulso * 4)
            )

            self.pantalla.blit(
                halo,
                (x - int(radio * 2.5), y - int(radio * 2.5))
            )

        surf = pygame.Surface((radio * 2 + 4, radio * 2 + 4), pygame.SRCALPHA)

        pygame.draw.circle(
            surf,
            color,
            (radio + 2, radio + 2),
            radio
        )

        pygame.draw.circle(
            surf,
            (255, 255, 255),
            (radio + 2, radio + 2),
            radio,
            3
        )

        if p["conectado_a"] is not None:
            pygame.draw.line(
                surf,
                (255, 255, 255),
                (radio - 6, radio + 2),
                (radio - 2, radio + 6),
                3
            )

            pygame.draw.line(
                surf,
                (255, 255, 255),
                (radio - 2, radio + 6),
                (radio + 8, radio - 6),
                3
            )

        self.pantalla.blit(surf, (x - radio - 2, y - radio - 2))

        etiqueta = self.fuente_etiqueta.render(
            p["protocolo"],
            True,
            (220, 230, 240)
        )

        etiqueta_x = x - etiqueta.get_width() // 2
        etiqueta_y = y + radio + 10

        pad_x = 8
        pad_y = 4

        fondo = pygame.Surface(
            (
                etiqueta.get_width() + pad_x * 2,
                etiqueta.get_height() + pad_y * 2
            ),
            pygame.SRCALPHA
        )

        fondo.fill((10, 16, 30, 200))

        self.pantalla.blit(
            fondo,
            (etiqueta_x - pad_x, etiqueta_y - pad_y)
        )

        pygame.draw.rect(
            self.pantalla,
            color,
            (
                etiqueta_x - pad_x,
                etiqueta_y - pad_y,
                etiqueta.get_width() + pad_x * 2,
                etiqueta.get_height() + pad_y * 2
            ),
            1,
            border_radius=4
        )

        self.pantalla.blit(etiqueta, (etiqueta_x, etiqueta_y))
