import math
import random
import pygame
from puzzles.puzzle_base import BasePuzzle

NAVE_W = 40
NAVE_H = 36
NAVE_VEL = 4.0

BALA_W = 6
BALA_H = 16
BALA_VEL = 9.0
COOLDOWN_DISPARO_MS = 180

PAQUETE_W = 26
PAQUETE_H = 20
PAQUETE_VEL_BASE = 1.8

VIRUS_RADIO = 16
VIRUS_HP_BASE = 2

COLORES_PAQUETES = [
    (120, 230, 160),
    (130, 220, 255),
    (255, 220, 120),
]

class PuzzleNave(BasePuzzle):
    TITULO = "ENTREGA DE PAQUETES"
    HINT = "WASD: mover  |  MOUSE: apuntar  |  CLICK IZQ: disparar  |  ESC: pausa"
    DURACION_INVENCIBLE_NAVE = 900
    DURACION_FLASH_PERDIDO = 600

    def _construir(self):
        if self.dificultad <= 2:
            self.max_paquetes_objetivo = 5
            self.vidas_nave = 4
            self.spawn_virus_ms = 2200
            self.vel_virus = 1.4
            self.max_paquetes_perdidos = 3
        elif self.dificultad <= 4:
            self.max_paquetes_objetivo = 8
            self.vidas_nave = 3
            self.spawn_virus_ms = 1500
            self.vel_virus = 1.8
            self.max_paquetes_perdidos = 3
        else:
            self.max_paquetes_objetivo = 10
            self.vidas_nave = 2
            self.spawn_virus_ms = 1100
            self.vel_virus = 2.2
            self.max_paquetes_perdidos = 2

        self.spawn_paquete_ms = 1400

        hud_h = 50
        self.play_rect = pygame.Rect(
            self.area.x + 10,
            self.area.y + hud_h,
            self.area.width - 20,
            self.area.height - hud_h - 10,
        )

        torre_w = 45
        torre_h = 64
        self.origen_rect = pygame.Rect(
            self.play_rect.x + 50,
            self.play_rect.centery - torre_h // 2,
            torre_w, torre_h,
        )
        self.destino_rect = pygame.Rect(
            self.play_rect.right - torre_w - 50,
            self.play_rect.centery - torre_h // 2,
            torre_w, torre_h,
        )

        self.nave = {
            "x": self.play_rect.centerx - 120,
            "y": self.play_rect.centery - NAVE_H // 2,
            "ancho": NAVE_W,
            "alto": NAVE_H,
            "color": (180, 220, 255),
            "vidas": self.vidas_nave,
            "invencible_hasta": 0,
            "angulo": -math.pi / 2,
        }
        self.nave_rect = pygame.Rect(
            int(self.nave["x"]), int(self.nave["y"]), NAVE_W, NAVE_H
        )

        self.paquetes = []
        self.virus = []
        self.balas = []
        self.particulas_explosion = []

        self.paquetes_entregados = 0
        self.paquetes_perdidos = 0
        self.virus_destruidos = 0
        self.virus_pasaron = 0

        self.tiempo_acumulado = 0
        self.tiempo_desde_spawn_paquete = 0
        self.tiempo_desde_spawn_virus = 0
        self.cooldown_disparo_restante = 0

        rng = random.Random(pygame.time.get_ticks())
        self.estrellas = []
        for _ in range(60):
            self.estrellas.append({
                "x": rng.randint(self.play_rect.x, self.play_rect.right),
                "y": rng.randint(self.play_rect.y, self.play_rect.bottom),
                "brillo": rng.randint(120, 255),
                "fase": rng.uniform(0, math.tau),
            })

        self.game_over_disparado = False

    def _verificar_victoria(self) -> bool:
        if self.game_over_disparado:
            return False
        return self.paquetes_entregados >= self.max_paquetes_objetivo

    def _manejar_evento(self, evento):
        if evento.type == pygame.MOUSEBUTTONDOWN and evento.button == 1:
            self._disparar()
        return None

    def _actualizar_subclase(self, dt):
        if self.estado != "JUGANDO":
            return

        self._chequear_derrota()
        if self.game_over_disparado:
            return

        dt_ms = dt
        dt_s = dt / 1000.0
        self.tiempo_acumulado += dt_ms
        self.tiempo_desde_spawn_paquete += dt_ms
        self.tiempo_desde_spawn_virus += dt_ms
        if self.cooldown_disparo_restante > 0:
            self.cooldown_disparo_restante -= dt_ms

        self._mover_nave_teclado(dt_s)
        self._actualizar_angulo_nave()

        if self.tiempo_desde_spawn_paquete >= self.spawn_paquete_ms:
            self.tiempo_desde_spawn_paquete = 0
            self._spawn_paquete()
        if self.tiempo_desde_spawn_virus >= self.spawn_virus_ms:
            self.tiempo_desde_spawn_virus = 0
            self._spawn_virus()

        for p in self.paquetes:
            p["x"] += p["vx"]
        for v in self.virus:
            v["x"] -= self.vel_virus
        for b in self.balas:
            b["x"] += math.cos(b["angulo"]) * BALA_VEL
            b["y"] += math.sin(b["angulo"]) * BALA_VEL

        nuevos_entregados = 0
        for p in self.paquetes:
            if p["estado"] == "vuelo" and p["x"] + PAQUETE_W >= self.destino_rect.x:
                p["estado"] = "entregado"
                nuevos_entregados += 1
                self._agregar_particulas(
                    p["x"] + PAQUETE_W // 2,
                    p["y"] + PAQUETE_H // 2,
                    (130, 230, 180), 8,
                )
        self.paquetes_entregados += nuevos_entregados

        for v in self.virus:
            if v["estado"] == "vivo" and v["x"] + VIRUS_RADIO * 2 < self.play_rect.x:
                v["estado"] = "fuera"
                self.virus_pasaron += 1

        self.paquetes = [p for p in self.paquetes
                         if p["estado"] == "vuelo"
                         and self.play_rect.x - 40 < p["x"] < self.play_rect.right + 40]
        self.virus = [v for v in self.virus if v["estado"] == "vivo"]
        self.balas = [b for b in self.balas
                      if (self.play_rect.x - 40 < b["x"] < self.destino_rect.x and
                          self.play_rect.y - 40 < b["y"] < self.play_rect.bottom + 40)]

        self._procesar_colisiones()

        for part in self.particulas_explosion:
            part["x"] += part["vx"]
            part["y"] += part["vy"]
            part["vida"] -= dt_ms
        self.particulas_explosion = [p for p in self.particulas_explosion if p["vida"] > 0]
        self._chequear_derrota()

    def _chequear_derrota(self):
        if self.game_over_disparado:
            return
        if self.nave["vidas"] <= 0 or self.paquetes_perdidos > self.max_paquetes_perdidos:
            self.game_over_disparado = True
            self._disparar_timeout()
            for p in self.paquetes:
                p["estado"] = "perdido"
            for v in self.virus:
                v["estado"] = "fuera"

    def _disparar_timeout(self):
        from puzzles.puzzle_base import _SalirPuzzle
        raise _SalirPuzzle("tiempo_agotado")

    def _mover_nave_teclado(self, dt_s):
        teclas = pygame.key.get_pressed()
        dx = 0
        dy = 0
        if teclas[pygame.K_w] or teclas[pygame.K_UP]:
            dy -= 1
        if teclas[pygame.K_s] or teclas[pygame.K_DOWN]:
            dy += 1
        if teclas[pygame.K_a] or teclas[pygame.K_LEFT]:
            dx -= 1
        if teclas[pygame.K_d] or teclas[pygame.K_RIGHT]:
            dx += 1

        if dx != 0 and dy != 0:
            inv = 1 / math.sqrt(2)
            dx *= inv
            dy *= inv

        paso = NAVE_VEL
        self.nave["x"] += dx * paso
        self.nave["y"] += dy * paso

        min_x = self.play_rect.x + 4
        max_x = self.play_rect.right - NAVE_W - 4
        min_y = self.play_rect.y + 4
        max_y = self.play_rect.bottom - NAVE_H - 4
        if self.nave["x"] < min_x:
            self.nave["x"] = min_x
        elif self.nave["x"] > max_x:
            self.nave["x"] = max_x
        if self.nave["y"] < min_y:
            self.nave["y"] = min_y
        elif self.nave["y"] > max_y:
            self.nave["y"] = max_y

        self.nave_rect.x = int(self.nave["x"])
        self.nave_rect.y = int(self.nave["y"])

    def _actualizar_angulo_nave(self):
        mouse_x, mouse_y = pygame.mouse.get_pos()
        nave_cx = self.nave["x"] + NAVE_W // 2
        nave_cy = self.nave["y"] + NAVE_H // 2
        dx = mouse_x - nave_cx
        dy = mouse_y - nave_cy
        self.nave["angulo"] = math.atan2(dy, dx)

    def _disparar(self):
        if self.cooldown_disparo_restante > 0:
            return
        if self.game_over_disparado:
            return
        nave_cx = self.nave["x"] + NAVE_W // 2
        nave_cy = self.nave["y"] + NAVE_H // 2

        offset_x = math.cos(self.nave["angulo"]) * (NAVE_H // 2)
        offset_y = math.sin(self.nave["angulo"]) * (NAVE_H // 2)
        self.balas.append({
            "x": nave_cx + offset_x - BALA_W // 2,
            "y": nave_cy + offset_y - BALA_H // 2,
            "angulo": self.nave["angulo"],
        })
        self.cooldown_disparo_restante = COOLDOWN_DISPARO_MS
        self._reproducir(self.sonido_interruptor)

    def _spawn_paquete(self):
        min_y = self.play_rect.y + 8
        max_y = self.play_rect.bottom - PAQUETE_H - 8
        y = random.randint(min_y, max_y)
        self.paquetes.append({
            "x": self.origen_rect.right + 2,
            "y": y,
            "vx": PAQUETE_VEL_BASE + random.uniform(-0.2, 0.4),
            "color": random.choice(COLORES_PAQUETES),
            "estado": "vuelo",
        })

    def _spawn_virus(self):
        min_y = self.play_rect.y + VIRUS_RADIO + 4
        max_y = self.play_rect.bottom - VIRUS_RADIO - 4
        y = random.randint(min_y, max_y)
        hp = VIRUS_HP_BASE + (1 if self.dificultad >= 4 else 0)
        self.virus.append({
            "x": self.destino_rect.x - VIRUS_RADIO,
            "y": y,
            "hp": hp,
            "estado": "vivo",
        })

    def _procesar_colisiones(self):
        for b in self.balas:
            br = pygame.Rect(int(b["x"]), int(b["y"]), BALA_W, BALA_H)
            for v in self.virus:
                if v["estado"] != "vivo":
                    continue
                vr = pygame.Rect(
                    int(v["x"] - VIRUS_RADIO), int(v["y"] - VIRUS_RADIO),
                    VIRUS_RADIO * 2, VIRUS_RADIO * 2,
                )
                if br.colliderect(vr):
                    v["hp"] -= 1
                    b["impacto"] = True
                    if v["hp"] <= 0:
                        v["estado"] = "muerto"
                        self.virus_destruidos += 1
                        self._agregar_particulas(
                            v["x"], v["y"], (255, 120, 120), 12
                        )
                        self._reproducir(self.sonido_exito)
                    else:
                        self._reproducir(self.sonido_error)
                    break

        self.balas = [b for b in self.balas if not b.get("impacto")]

        nuevos_perdidos = 0
        for v in self.virus:
            if v["estado"] != "vivo":
                continue
            vr = pygame.Rect(
                int(v["x"] - VIRUS_RADIO), int(v["y"] - VIRUS_RADIO),
                VIRUS_RADIO * 2, VIRUS_RADIO * 2,
            )
            for p in self.paquetes:
                if p["estado"] != "vuelo":
                    continue
                pr = pygame.Rect(int(p["x"]), int(p["y"]), PAQUETE_W, PAQUETE_H)
                if vr.colliderect(pr):
                    p["estado"] = "perdido"
                    v["estado"] = "muerto"
                    nuevos_perdidos += 1
                    self._agregar_particulas(
                        p["x"] + PAQUETE_W // 2, p["y"] + PAQUETE_H // 2,
                        (255, 100, 100), 10,
                    )
                    self._reproducir(self.sonido_error)
                    break
        self.paquetes_perdidos += nuevos_perdidos

        if pygame.time.get_ticks() >= self.nave["invencible_hasta"]:
            for v in self.virus:
                if v["estado"] != "vivo":
                    continue
                vr = pygame.Rect(
                    int(v["x"] - VIRUS_RADIO), int(v["y"] - VIRUS_RADIO),
                    VIRUS_RADIO * 2, VIRUS_RADIO * 2,
                )
                if vr.colliderect(self.nave_rect):
                    self.nave["vidas"] -= 1
                    self.nave["invencible_hasta"] = (
                        pygame.time.get_ticks() + self.DURACION_INVENCIBLE_NAVE
                    )
                    v["estado"] = "muerto"
                    self._agregar_particulas(
                        self.nave_rect.centerx, self.nave_rect.centery,
                        (255, 150, 150), 14,
                    )
                    self._reproducir(self.sonido_error)
                    break

    def _agregar_particulas(self, x, y, color, cantidad):
        for _ in range(cantidad):
            angulo = random.uniform(0, math.tau)
            vel = random.uniform(0.8, 2.6)
            self.particulas_explosion.append({
                "x": x,
                "y": y,
                "vx": math.cos(angulo) * vel,
                "vy": math.sin(angulo) * vel,
                "vida": random.randint(350, 700),
                "color": color,
            })

    def _dibujar_subclase(self):
        self._dibujar_fondo_y_torres()
        self._dibujar_hud()

        for p in self.paquetes:
            if p["estado"] == "vuelo":
                self._dibujar_paquete(p)

        for v in self.virus:
            if v["estado"] == "vivo":
                self._dibujar_virus(v)

        for b in self.balas:
            self._dibujar_bala(b)

        self._dibujar_nave()
        self._dibujar_particulas()

    def _dibujar_fondo_y_torres(self):
        fondo = pygame.Surface((self.play_rect.width, self.play_rect.height), pygame.SRCALPHA)
        fondo.fill((8, 14, 28, 230))
        self.pantalla.blit(fondo, self.play_rect.topleft)
        pygame.draw.rect(
            self.pantalla, (60, 90, 140),
            self.play_rect, 1, border_radius=6,
        )

        ahora = pygame.time.get_ticks() / 1000.0
        for e in self.estrellas:
            intensidad = int(
                e["brillo"] * (0.6 + 0.4 * math.sin(ahora * 2.0 + e["fase"]))
            )
            pygame.draw.circle(
                self.pantalla,
                (intensidad, intensidad, min(255, intensidad + 20)),
                (e["x"], e["y"]), 1,
            )

        self._dibujar_torre_origen()
        self._dibujar_torre_destino()

    def _dibujar_torre_origen(self):
        r = self.origen_rect
        pygame.draw.rect(self.pantalla, (40, 80, 50), r, border_radius=6)
        pygame.draw.rect(self.pantalla, (120, 200, 140), r, 2, border_radius=6)
        antena_top = (r.centerx, r.y - 18)
        pygame.draw.line(self.pantalla, (120, 200, 140),
                         (r.centerx, r.y + 4), antena_top, 2)
        pulso = (math.sin(pygame.time.get_ticks() * 0.006) + 1) / 2
        radio_luz = 3 + int(pulso * 2)
        pygame.draw.circle(self.pantalla, (140, 255, 180), antena_top, radio_luz)
        etiq = self.fuente_peq.render("ORIGEN", True, (180, 255, 210))
        self.pantalla.blit(
            etiq,
            (r.centerx - etiq.get_width() // 2, r.bottom + 4),
        )

    def _dibujar_torre_destino(self):
        r = self.destino_rect
        pygame.draw.rect(self.pantalla, (30, 60, 90), r, border_radius=6)
        pygame.draw.rect(self.pantalla, (100, 200, 240), r, 2, border_radius=6)
        for i in range(3):
            ly = r.y + 12 + i * 16
            pygame.draw.rect(
                self.pantalla, (60, 100, 140),
                (r.x + 6, ly, r.width - 12, 8), border_radius=2,
            )
            color_led = (120, 255, 180) if i % 2 == 0 else (255, 220, 120)
            pygame.draw.circle(
                self.pantalla, color_led,
                (r.right - 10, ly + 4), 2,
            )
        etiq = self.fuente_peq.render("DESTINO", True, (180, 230, 255))
        self.pantalla.blit(
            etiq,
            (r.centerx - etiq.get_width() // 2, r.bottom + 4),
        )

    def _dibujar_hud(self):
        hud_rect = pygame.Rect(
            self.area.x + 10,
            self.area.y + 5,
            self.area.width - 20,
            40,
        )
        fondo = pygame.Surface((hud_rect.width, hud_rect.height), pygame.SRCALPHA)
        fondo.fill((20, 30, 50, 200))
        self.pantalla.blit(fondo, hud_rect.topleft)
        pygame.draw.rect(
            self.pantalla, (90, 120, 160),
            hud_rect, 1, border_radius=6,
        )

        paquetes_txt = f"ENTREGADOS {self.paquetes_entregados}/{self.max_paquetes_objetivo}"
        perdidos_txt = f"PERDIDOS {self.paquetes_perdidos}/{self.max_paquetes_perdidos}"
        virus_txt = f"VIRUS {self.virus_destruidos}"
        vidas_txt = f"VIDAS {self.nave['vidas']}"

        color_paq = (140, 255, 200) if self.paquetes_entregados < self.max_paquetes_objetivo else (120, 255, 180)
        color_perd = (255, 120, 120) if self.paquetes_perdidos >= self.max_paquetes_perdidos else (255, 200, 120)

        def _blit_texto(texto, x, y, color):
            render = self.fuente_peq.render(texto, True, color)
            self.pantalla.blit(render, (x, y))
            return render.get_width()

        x = hud_rect.x + 12
        y = hud_rect.y + (hud_rect.height - 16) // 2
        w = _blit_texto(paquetes_txt, x, y, color_paq)
        x += w + 24
        w = _blit_texto(perdidos_txt, x, y, color_perd)
        x += w + 24
        w = _blit_texto(virus_txt, x, y, (200, 150, 255))
        x += w + 24
        _blit_texto(vidas_txt, x, y, (180, 220, 255))

    def _dibujar_paquete(self, p):
        x, y = int(p["x"]), int(p["y"])
        color = p["color"]
        caja = pygame.Surface((PAQUETE_W, PAQUETE_H), pygame.SRCALPHA)
        caja.fill((*color, 80))
        pygame.draw.rect(caja, color, (0, 0, PAQUETE_W, PAQUETE_H), 2, border_radius=4)
        pygame.draw.line(caja, color, (3, 5), (PAQUETE_W // 2, PAQUETE_H // 2), 1)
        pygame.draw.line(caja, color, (PAQUETE_W - 3, 5), (PAQUETE_W // 2, PAQUETE_H // 2), 1)
        for i in range(3):
            cx = x - 4 - i * 5
            cy = y + PAQUETE_H // 2
            alpha = max(40, 180 - i * 60)
            s = pygame.Surface((4, 2), pygame.SRCALPHA)
            s.fill((*color, alpha))
            self.pantalla.blit(s, (cx, cy))
        self.pantalla.blit(caja, (x, y))

    def _dibujar_virus(self, v):
        cx, cy = int(v["x"]), int(v["y"])
        radio = VIRUS_RADIO

        pygame.draw.circle(self.pantalla, (200, 40, 60), (cx, cy), radio)
        pygame.draw.circle(self.pantalla, (255, 120, 140), (cx, cy), radio, 2)
        for i in range(8):
            angulo = i * (math.tau / 8) + pygame.time.get_ticks() * 0.001
            x1 = cx + math.cos(angulo) * (radio - 2)
            y1 = cy + math.sin(angulo) * (radio - 2)
            x2 = cx + math.cos(angulo) * (radio + 6)
            y2 = cy + math.sin(angulo) * (radio + 6)
            pygame.draw.line(self.pantalla, (255, 140, 160), (x1, y1), (x2, y2), 2)
        pygame.draw.circle(self.pantalla, (255, 255, 200), (cx - 4, cy - 3), 2)
        pygame.draw.circle(self.pantalla, (255, 255, 200), (cx + 4, cy - 3), 2)
        pygame.draw.circle(self.pantalla, (0, 0, 0), (cx - 4, cy - 3), 1)
        pygame.draw.circle(self.pantalla, (0, 0, 0), (cx + 4, cy - 3), 1)
        if v["hp"] > 1:
            bw = 20
            bh = 3
            bx = cx - bw // 2
            by = cy - radio - 8
            pygame.draw.rect(self.pantalla, (60, 20, 30), (bx, by, bw, bh))
            pygame.draw.rect(
                self.pantalla, (255, 200, 80),
                (bx, by, int(bw * v["hp"] / VIRUS_HP_BASE), bh),
            )

    def _dibujar_bala(self, b):
        x, y = int(b["x"]), int(b["y"])
        angulo = b.get("angulo", -math.pi / 2)

        surf_size = max(BALA_W, BALA_H) * 2
        bala_surf = pygame.Surface((surf_size, surf_size), pygame.SRCALPHA)
        center = surf_size // 2

        halo_rect = pygame.Rect(center - BALA_W // 2 - 3,
                                 center - BALA_H // 2 - 3,
                                 BALA_W + 6, BALA_H + 6)
        pygame.draw.ellipse(bala_surf, (180, 240, 255, 100), halo_rect)

        bala_rect = pygame.Rect(center - BALA_W // 2,
                                 center - BALA_H // 2,
                                 BALA_W, BALA_H)
        pygame.draw.rect(bala_surf, (220, 245, 255), bala_rect, border_radius=2)
        pygame.draw.rect(bala_surf, (140, 220, 255), bala_rect, 1, border_radius=2)

        angulo_grados = math.degrees(angulo + math.pi / 2)
        bala_rotada = pygame.transform.rotate(bala_surf, -angulo_grados)
        rot_rect = bala_rotada.get_rect(center=(x + BALA_W // 2, y + BALA_H // 2))

        self.pantalla.blit(bala_rotada, rot_rect)

    def _dibujar_nave(self):
        cx = int(self.nave["x"] + NAVE_W // 2)
        cy = int(self.nave["y"] + NAVE_H // 2)

        ahora = pygame.time.get_ticks()
        if ahora < self.nave["invencible_hasta"]:
            if (ahora // 80) % 2 == 0:
                return

        color = self.nave["color"]
        angulo = self.nave["angulo"]

        nave_w, nave_h = NAVE_W, NAVE_H
        surf_size = max(nave_w, nave_h) * 2
        nave_surf = pygame.Surface((surf_size, surf_size), pygame.SRCALPHA)
        center_surf = surf_size // 2

        ala_pts_izq = [
            (center_surf + 4, center_surf + nave_h // 2 - 6),
            (center_surf, center_surf + nave_h // 2),
            (center_surf + 8, center_surf + nave_h // 2),
        ]
        ala_pts_der = [
            (center_surf + nave_w - 4, center_surf + nave_h // 2 - 6),
            (center_surf + nave_w, center_surf + nave_h // 2),
            (center_surf + nave_w - 8, center_surf + nave_h // 2),
        ]
        pygame.draw.polygon(nave_surf, (110, 170, 220), ala_pts_izq)
        pygame.draw.polygon(nave_surf, (110, 170, 220), ala_pts_der)
        pygame.draw.polygon(nave_surf, color, ala_pts_izq, 1)
        pygame.draw.polygon(nave_surf, color, ala_pts_der, 1)

        cuerpo = [
            (center_surf + nave_w // 2, center_surf - nave_h // 2),
            (center_surf + nave_w - 4, center_surf + nave_h // 2 - 4),
            (center_surf + 4, center_surf + nave_h // 2 - 4),
        ]
        pygame.draw.polygon(nave_surf, (40, 80, 130), cuerpo)
        pygame.draw.polygon(nave_surf, color, cuerpo, 2)

        cabina = [
            (center_surf + nave_w // 2, center_surf - nave_h // 2 + 6),
            (center_surf + nave_w // 2 + 5, center_surf + nave_h // 2 - 10),
            (center_surf + nave_w // 2 - 5, center_surf + nave_h // 2 - 10),
        ]
        pygame.draw.polygon(nave_surf, (140, 230, 255), cabina)

        pulso = (math.sin(ahora * 0.02) + 1) / 2
        largo_llama = 6 + int(pulso * 5)
        llama = [
            (center_surf + nave_w // 2 - 3, center_surf + nave_h // 2 - 2),
            (center_surf + nave_w // 2 + 3, center_surf + nave_h // 2 - 2),
            (center_surf + nave_w // 2, center_surf + nave_h // 2 - 2 + largo_llama),
        ]
        pygame.draw.polygon(nave_surf, (255, 180, 80), llama)

        angulo_grados = math.degrees(angulo + math.pi / 2)
        nave_rotada = pygame.transform.rotate(nave_surf, -angulo_grados)
        rot_rect = nave_rotada.get_rect(center=(cx, cy))

        self.pantalla.blit(nave_rotada, rot_rect)

        self.nave_rect = pygame.Rect(
            cx - NAVE_W // 2, cy - NAVE_H // 2, NAVE_W, NAVE_H
        )

    def _dibujar_particulas(self):
        for part in self.particulas_explosion:
            x, y = int(part["x"]), int(part["y"])
            vida_ratio = max(0, min(1, part["vida"] / 700))
            radio = max(1, int(3 * vida_ratio))
            alpha = int(220 * vida_ratio)
            s = pygame.Surface((radio * 2, radio * 2), pygame.SRCALPHA)
            pygame.draw.circle(
                s, (*part["color"], alpha),
                (radio, radio), radio,
            )
            self.pantalla.blit(s, (x - radio, y - radio))
