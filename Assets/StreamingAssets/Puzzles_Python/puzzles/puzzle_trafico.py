"""
PuzzleTrafico — clasificador estilo Wireshark.
El jugador debe arrastrar paquetes al bin del protocolo correcto.
"""

import math
import random
import pygame
from puzzles.puzzle_base import BasePuzzle

BINS = [
    {"nombre": "HTTP", "color": (255, 200, 100)},
    {"nombre": "DNS",  "color": (180, 130, 255)},
    {"nombre": "FTP",  "color": (130, 220, 180)},
    {"nombre": "SSH",  "color": (255, 150, 200)},
]

PAYLOADS = {
    "HTTP": ["GET /index.html", "POST /login", "PUT /api/v1/users", "HTTP/1.1 200 OK", "GET /styles.css"],
    "DNS":  ["DNS ?google.com", "DNS ?youtube.com", "DNS A example.org", "DNS PTR 8.8.8.8", "DNS MX gmail.com"],
    "FTP":  ["USER admin", "PASS ****", "RETR file.zip", "LIST -la", "STOR upload.txt"],
    "SSH":  ["SSH-2.0-OpenSSH", "RSA key exchange", "AES-256-GCM", "auth password", "client banner"],
}


class PuzzleTrafico(BasePuzzle):
    TITULO = "TRAFICO DE RED"
    HINT = "Arrastra cada paquete al bin del protocolo correcto  |  ESC pausa"
    DURACION_ERROR = 400

    def _construir(self):
        if self.dificultad <= 2:
            n_paquetes = 4
        elif self.dificultad <= 4:
            n_paquetes = 6
        else:
            n_paquetes = 7

        rng = random.Random(pygame.time.get_ticks())

        self.paquetes = []
        protocolos_elegidos = []
        if n_paquetes >= 4:
            protocolos_elegidos = [b["nombre"] for b in BINS]
            for _ in range(n_paquetes - 4):
                protocolos_elegidos.append(rng.choice([b["nombre"] for b in BINS]))
            rng.shuffle(protocolos_elegidos)
        else:
            protocolos_elegidos = rng.choices([b["nombre"] for b in BINS], k=n_paquetes)

        for i, prot in enumerate(protocolos_elegidos):
            color_bin = next(b["color"] for b in BINS if b["nombre"] == prot)
            snippet = rng.choice(PAYLOADS[prot])
            self.paquetes.append({
                "id": i,
                "protocolo": prot,
                "color": color_bin,
                "snippet": snippet,
                "estado": "cola",
                "x": 0, "y": 0,
                "bin_id": None,
            })

        paquete_w = 280
        paquete_h = 56
        gap_y = 14
        x_cola = self.area.x + 30
        y_inicio = self.area.y + 30
        total_h = n_paquetes * (paquete_h + gap_y)
        if total_h > self.area.height - 60:
            paso_real = (self.area.height - 60 - paquete_h) // max(1, n_paquetes)
            gap_y = max(6, paso_real - paquete_h)
            y_inicio = self.area.y + 30

        idx_cola = 0
        for p in self.paquetes:
            if p["estado"] != "cola":
                continue
            p["x"] = x_cola
            p["y"] = y_inicio + idx_cola * (paquete_h + gap_y)
            idx_cola += 1

        self.bins = []
        n_bins = len(BINS)
        bin_w = 240
        bin_h = 140
        cols_bins = 2
        filas_bins = 2
        grid_w = cols_bins * bin_w + (cols_bins - 1) * 30
        grid_h = filas_bins * bin_h + (filas_bins - 1) * 30
        grid_x = self.area.x + self.area.width - grid_w - 30
        grid_y = self.area.y + (self.area.height - grid_h) // 2

        for i, b in enumerate(BINS):
            col = i % cols_bins
            fila = i // cols_bins
            bx = grid_x + col * (bin_w + 30)
            by = grid_y + fila * (bin_h + 30)
            self.bins.append({
                "id": i,
                "nombre": b["nombre"],
                "color": b["color"],
                "rect": pygame.Rect(bx, by, bin_w, bin_h),
                "flash_color": None,
                "flash_tiempo": 0,
            })

        self.paquete_arrastrado = None

    def _verificar_victoria(self):
        return all(p["estado"] == "depositado" for p in self.paquetes)

    def _manejar_evento(self, evento):
        if evento.type != pygame.MOUSEBUTTONDOWN or evento.button != 1:
            return None

        mouse_pos = pygame.mouse.get_pos()

        if self.paquete_arrastrado is not None:
            for b in self.bins:
                if b["rect"].collidepoint(mouse_pos):
                    self._soltar_en_bin(self.paquete_arrastrado, b["id"])
                    return None
            self._cancelar_agarre()
            return None

        for p in self.paquetes:
            if p["estado"] != "cola":
                continue
            rect = pygame.Rect(p["x"], p["y"], 280, 56)
            if rect.collidepoint(mouse_pos):
                self.paquete_arrastrado = p["id"]
                p["estado"] = "agarrado"
                return None

        return None

    def _cancelar_agarre(self):
        if self.paquete_arrastrado is None:
            return
        p = self.paquetes[self.paquete_arrastrado]
        p["estado"] = "cola"
        self._recolocar_paquete(p)
        self.paquete_arrastrado = None

    def _soltar_en_bin(self, pid, bin_id):
        p = self.paquetes[pid]
        b = self.bins[bin_id]

        if p["protocolo"] == b["nombre"]:
            p["estado"] = "depositado"
            p["bin_id"] = bin_id
            p["x"] = b["rect"].x + 30
            p["y"] = b["rect"].bottom - 50
            self.paquete_arrastrado = None
            b["flash_color"] = (120, 255, 180)
            b["flash_tiempo"] = pygame.time.get_ticks()
            self._reproducir(self.sonido_conectar)
        else:
            p["estado"] = "cola"
            self.paquete_arrastrado = None
            self._recolocar_paquete(p)
            b["flash_color"] = (255, 80, 80)
            b["flash_tiempo"] = pygame.time.get_ticks()
            self._reproducir(self.sonido_error)

    def _recolocar_paquete(self, p):
        idx = 0
        for otro in self.paquetes:
            if otro["estado"] != "cola":
                continue
            if otro["id"] == p["id"]:
                break
            idx += 1
        gap_y = 14
        x_cola = self.area.x + 30
        y_inicio = self.area.y + 30
        p["x"] = x_cola
        p["y"] = y_inicio + idx * (56 + gap_y)

    def _actualizar_subclase(self, dt):
        ahora = pygame.time.get_ticks()
        for b in self.bins:
            if b["flash_color"] is not None and ahora - b["flash_tiempo"] > self.DURACION_ERROR:
                b["flash_color"] = None

    def _dibujar_subclase(self):
        for b in self.bins:
            self._dibujar_bin(b)

        for p in self.paquetes:
            if p["estado"] == "agarrado":
                continue
            if p["estado"] == "depositado":
                continue
            self._dibujar_paquete(p, alpha=255)

        if self.paquete_arrastrado is not None:
            p = self.paquetes[self.paquete_arrastrado]
            mx, my = pygame.mouse.get_pos()
            self._dibujar_paquete(p, alpha=240, x_override=mx - 140, y_override=my - 28)

    def _dibujar_paquete(self, p, alpha=255, x_override=None, y_override=None):
        x = x_override if x_override is not None else p["x"]
        y = y_override if y_override is not None else p["y"]
        w, h = 280, 56
        color = p["color"]

        surf = pygame.Surface((w, h), pygame.SRCALPHA)
        surf.fill((*color, 50))
        pygame.draw.rect(surf, color, (0, 0, w, h), 2, border_radius=8)
        pygame.draw.rect(surf, color, (0, 0, 6, h), border_radius=3)

        header_texto = f"{p['protocolo']}"
        header_render = self.fuente_peq.render(header_texto, True, color)
        surf.blit(header_render, (14, 8))

        payload_render = self.fuente_peq.render(p["snippet"], True, (240, 240, 250))
        surf.blit(payload_render, (14, 28))

        surf.set_alpha(alpha)
        self.pantalla.blit(surf, (x, y))

    def _dibujar_bin(self, b):
        rect = b["rect"]
        color = b["color"]
        mouse_pos = pygame.mouse.get_pos()
        hover = rect.collidepoint(mouse_pos) and self.paquete_arrastrado is not None

        if hover:
            halo = pygame.Surface((rect.width + 20, rect.height + 20), pygame.SRCALPHA)
            pygame.draw.rect(
                halo, (*color, 100),
                (10, 10, rect.width, rect.height), border_radius=14
            )
            self.pantalla.blit(halo, (rect.x - 10, rect.y - 10))

        fondo = pygame.Surface((rect.width, rect.height), pygame.SRCALPHA)
        fondo.fill((*color, 30))
        pygame.draw.rect(fondo, color, (0, 0, rect.width, rect.height), 2, border_radius=12)

        etiqueta = self.fuente_etiqueta.render(b["nombre"], True, color)
        fondo.blit(
            etiqueta,
            (rect.width // 2 - etiqueta.get_width() // 2, 16)
        )

        pygame.draw.line(
            fondo, (*color, 120),
            (20, 50), (rect.width - 20, 50), 1
        )

        drop = self.fuente_peq.render("DROP HERE", True, (*color, 180))
        fondo.blit(
            drop,
            (rect.width // 2 - drop.get_width() // 2, rect.height - 28)
        )

        self.pantalla.blit(fondo, rect.topleft)

        if b["flash_color"] is not None:
            flash = pygame.Surface((rect.width, rect.height), pygame.SRCALPHA)
            flash.fill((*b["flash_color"], 120))
            pygame.draw.rect(flash, b["flash_color"], (0, 0, rect.width, rect.height), 4, border_radius=12)
            self.pantalla.blit(flash, rect.topleft)
