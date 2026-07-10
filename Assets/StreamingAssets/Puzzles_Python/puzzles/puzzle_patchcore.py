import math
import random
import pygame
from puzzles.puzzle_base import BasePuzzle

COLORES_PATCHCORD = [
    (255, 90, 90),
    (255, 180, 70),
    (255, 230, 90),
    (120, 230, 150),
    (90, 210, 255),
    (150, 150, 255),
    (220, 130, 255),
    (255, 130, 190),
    (130, 245, 230),
    (220, 230, 240),
]


class PuzzlePatchcore(BasePuzzle):
    TITULO = "PATCHCORD ROTO"
    HINT = "Click pieza y luego espacio para reconstruir el patchcord  |  ESC pausa"
    DURACION_ERROR = 360

    def _construir(self):
        if self.dificultad <= 2:
            self.total_piezas = 6
        elif self.dificultad <= 4:
            self.total_piezas = 8
        else:
            self.total_piezas = 10

        rng = random.Random(pygame.time.get_ticks())
        colores = COLORES_PATCHCORD[:self.total_piezas]
        self.piezas = []
        for i in range(self.total_piezas):
            self.piezas.append({
                "id": i,
                "orden": i,
                "color": colores[i],
                "slot": None,
                "rect": pygame.Rect(0, 0, 1, 1),
            })

        self.banco = [p["id"] for p in self.piezas]
        rng.shuffle(self.banco)
        if self.banco == list(range(self.total_piezas)):
            rng.shuffle(self.banco)

        self.slots = [None for _ in range(self.total_piezas)]
        self.slot_rects = [pygame.Rect(0, 0, 1, 1) for _ in range(self.total_piezas)]
        self.seleccion = None
        self.error_slots = []
        self.error_tiempo = 0
        self._layout_cache = None

    def _verificar_victoria(self):
        return all(pid is not None and self.piezas[pid]["orden"] == idx for idx, pid in enumerate(self.slots))

    def _manejar_evento(self, evento):
        if evento.type != pygame.MOUSEBUTTONDOWN or evento.button != 1:
            return None

        self._actualizar_layout()
        mouse_pos = pygame.mouse.get_pos()

        for idx, rect in enumerate(self.slot_rects):
            if rect.collidepoint(mouse_pos):
                self._click_slot(idx)
                return None

        for pieza in self.piezas:
            if pieza["rect"].collidepoint(mouse_pos):
                self._click_pieza(pieza["id"])
                return None

        self.seleccion = None
        return None

    def _click_pieza(self, pid):
        if self.seleccion == pid:
            self.seleccion = None
        else:
            self.seleccion = pid

    def _click_slot(self, slot_idx):
        if self.seleccion is None:
            pieza_en_slot = self.slots[slot_idx]
            if pieza_en_slot is not None:
                self.seleccion = pieza_en_slot
            return

        pieza_id = self.seleccion
        slot_actual = self.piezas[pieza_id]["slot"]
        pieza_destino = self.slots[slot_idx]

        if slot_actual is not None:
            self.slots[slot_actual] = pieza_destino
        else:
            self.banco = [pid for pid in self.banco if pid != pieza_id]

        if pieza_destino is not None:
            self.piezas[pieza_destino]["slot"] = slot_actual
            if slot_actual is None:
                self.banco.append(pieza_destino)

        self.slots[slot_idx] = pieza_id
        self.piezas[pieza_id]["slot"] = slot_idx
        self.seleccion = None
        self._layout_cache = None

        if pieza_id != slot_idx:
            self.error_slots = [slot_idx]
            self.error_tiempo = pygame.time.get_ticks()
            self._reproducir(self.sonido_error)
        else:
            self._reproducir(self.sonido_conectar)

    def _actualizar_subclase(self, dt):
        if self.error_slots and pygame.time.get_ticks() - self.error_tiempo > self.DURACION_ERROR:
            self.error_slots = []

    def _actualizar_layout(self):
        key = (self.area.x, self.area.y, self.area.width, self.area.height, self.total_piezas)
        if self._layout_cache == key:
            return
        self._layout_cache = key

        gap = 8
        slot_h = 68
        usable_w = self.area.width - 80
        slot_w = max(58, min(96, (usable_w - gap * (self.total_piezas - 1)) // self.total_piezas))
        total_w = slot_w * self.total_piezas + gap * (self.total_piezas - 1)
        start_x = self.area.centerx - total_w // 2
        slot_y = self.area.y + int(self.area.height * 0.24)

        self.slot_rects = []
        for i in range(self.total_piezas):
            self.slot_rects.append(pygame.Rect(start_x + i * (slot_w + gap), slot_y, slot_w, slot_h))

        banco_y = self.area.y + int(self.area.height * 0.64)
        banco_ids = self.banco[:]
        cols = min(self.total_piezas, 5)
        rows = math.ceil(len(banco_ids) / cols) if banco_ids else 1
        piece_w = slot_w
        piece_h = 58
        row_gap = 12
        col_gap = 12
        grid_w = cols * piece_w + (cols - 1) * col_gap
        base_x = self.area.centerx - grid_w // 2

        for idx, pid in enumerate(banco_ids):
            col = idx % cols
            row = idx // cols
            self.piezas[pid]["rect"] = pygame.Rect(
                base_x + col * (piece_w + col_gap),
                banco_y + row * (piece_h + row_gap),
                piece_w,
                piece_h,
            )

        for slot_idx, pid in enumerate(self.slots):
            if pid is None:
                continue
            rect = self.slot_rects[slot_idx].inflate(-8, -10)
            self.piezas[pid]["rect"] = rect

    def _dibujar_subclase(self):
        self._actualizar_layout()
        self._dibujar_mesa()
        self._dibujar_patchcord_base()

        for idx, rect in enumerate(self.slot_rects):
            self._dibujar_slot(idx, rect)

        for pid in self.banco:
            self._dibujar_pieza(self.piezas[pid])

        for pid in self.slots:
            if pid is not None:
                self._dibujar_pieza(self.piezas[pid])

    def _dibujar_mesa(self):
        mesa = pygame.Surface((self.area.width, self.area.height), pygame.SRCALPHA)
        mesa.fill((8, 15, 28, 180))
        self.pantalla.blit(mesa, self.area.topleft)
        pygame.draw.rect(self.pantalla, (50, 90, 130), self.area, 1, border_radius=8)

    def _dibujar_patchcord_base(self):
        y = self.slot_rects[0].centery
        x1 = self.slot_rects[0].left - 64
        x2 = self.slot_rects[-1].right + 64
        pygame.draw.line(self.pantalla, (30, 40, 55), (x1, y), (x2, y), 18)
        pygame.draw.line(self.pantalla, (120, 150, 180), (x1, y), (x2, y), 6)

        for x, texto in ((x1 - 18, "RJ45"), (x2 + 18, "RJ45")):
            rect = pygame.Rect(0, 0, 70, 52)
            rect.center = (x, y)
            pygame.draw.rect(self.pantalla, (36, 52, 70), rect, border_radius=8)
            pygame.draw.rect(self.pantalla, (120, 210, 255), rect, 2, border_radius=8)
            for i in range(4):
                pin_x = rect.x + 12 + i * 12
                pygame.draw.rect(self.pantalla, (240, 210, 100), (pin_x, rect.y + 10, 6, 10), border_radius=2)
            label = self.fuente_etiqueta.render(texto, True, (190, 230, 255))
            self.pantalla.blit(label, label.get_rect(center=(rect.centerx, rect.bottom + 14)))

    def _dibujar_slot(self, idx, rect):
        ocupado = self.slots[idx] is not None
        color = (70, 90, 115) if not ocupado else (100, 130, 160)
        if idx in self.error_slots:
            pulso = (math.sin(pygame.time.get_ticks() * 0.04) + 1) / 2
            color = (255, 70 + int(pulso * 60), 70)

        pygame.draw.rect(self.pantalla, (12, 20, 35), rect, border_radius=8)
        pygame.draw.rect(self.pantalla, color, rect, 2, border_radius=8)

        numero = self.fuente_etiqueta.render(str(idx + 1), True, (140, 165, 190))
        self.pantalla.blit(numero, numero.get_rect(center=(rect.centerx, rect.bottom + 16)))

    def _dibujar_pieza(self, pieza):
        rect = pieza["rect"]
        color = pieza["color"]
        seleccionado = self.seleccion == pieza["id"]

        if seleccionado:
            halo = pygame.Surface((rect.width + 28, rect.height + 28), pygame.SRCALPHA)
            pygame.draw.rect(halo, (*color, 110), (14, 14, rect.width, rect.height), border_radius=10)
            self.pantalla.blit(halo, (rect.x - 14, rect.y - 14))

        cuerpo = pygame.Surface((rect.width, rect.height), pygame.SRCALPHA)
        cuerpo.fill((*color, 70))
        pygame.draw.rect(cuerpo, color, cuerpo.get_rect(), border_radius=8)
        pygame.draw.rect(cuerpo, (255, 255, 255), cuerpo.get_rect(), 2, border_radius=8)

        for i in range(3):
            y = 12 + i * 14
            pygame.draw.line(cuerpo, (20, 30, 42), (8, y), (rect.width - 8, y), 3)
            pygame.draw.line(cuerpo, (255, 255, 255, 180), (12, y - 3), (rect.width - 12, y - 3), 1)

        codigo = f"{pieza['orden'] + 1:02}"
        texto = self.fuente_etiqueta.render(codigo, True, (8, 12, 20))
        cuerpo.blit(texto, texto.get_rect(center=(rect.width // 2, rect.height // 2)))
        self.pantalla.blit(cuerpo, rect.topleft)
