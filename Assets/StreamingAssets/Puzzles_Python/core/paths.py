import os

BASE_DIR = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

def asset_path(*parts):
    return os.path.join(BASE_DIR, "assets", *parts)

def puzzle_path(puzzle_name):
    return os.path.join(BASE_DIR, "puzzles", puzzle_name)

def resultado_path():
    return os.path.join(BASE_DIR, "resultado.txt")

def get_base_dir():
    return BASE_DIR
