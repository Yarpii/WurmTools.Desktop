namespace WurmTools.Data;

public static class DatabaseSchema
{
    public const string CreateTables = """
        CREATE TABLE IF NOT EXISTS items (
            id          INTEGER PRIMARY KEY AUTOINCREMENT,
            name        TEXT NOT NULL,
            description TEXT,
            category    TEXT,
            material    TEXT,
            weight      REAL,
            skill       TEXT,
            difficulty  REAL,
            is_container INTEGER NOT NULL DEFAULT 0,
            is_improved  INTEGER NOT NULL DEFAULT 0,
            wiki_url    TEXT,
            notes       TEXT
        );

        CREATE INDEX IF NOT EXISTS idx_items_name ON items(name);
        CREATE INDEX IF NOT EXISTS idx_items_category ON items(category);
        CREATE INDEX IF NOT EXISTS idx_items_skill ON items(skill);

        -- FTS5 virtual table for full-text search
        CREATE VIRTUAL TABLE IF NOT EXISTS items_fts USING fts5(
            name,
            description,
            category,
            material,
            skill,
            notes,
            content='items',
            content_rowid='id'
        );

        -- Triggers to keep FTS index in sync
        CREATE TRIGGER IF NOT EXISTS items_ai AFTER INSERT ON items BEGIN
            INSERT INTO items_fts(rowid, name, description, category, material, skill, notes)
            VALUES (new.id, new.name, new.description, new.category, new.material, new.skill, new.notes);
        END;

        CREATE TRIGGER IF NOT EXISTS items_ad AFTER DELETE ON items BEGIN
            INSERT INTO items_fts(items_fts, rowid, name, description, category, material, skill, notes)
            VALUES ('delete', old.id, old.name, old.description, old.category, old.material, old.skill, old.notes);
        END;

        CREATE TRIGGER IF NOT EXISTS items_au AFTER UPDATE ON items BEGIN
            INSERT INTO items_fts(items_fts, rowid, name, description, category, material, skill, notes)
            VALUES ('delete', old.id, old.name, old.description, old.category, old.material, old.skill, old.notes);
            INSERT INTO items_fts(rowid, name, description, category, material, skill, notes)
            VALUES (new.id, new.name, new.description, new.category, new.material, new.skill, new.notes);
        END;

        CREATE TABLE IF NOT EXISTS crafting_recipes (
            id              INTEGER PRIMARY KEY AUTOINCREMENT,
            result_item_id  INTEGER NOT NULL,
            skill           TEXT,
            min_skill_level REAL,
            notes           TEXT,
            FOREIGN KEY (result_item_id) REFERENCES items(id)
        );

        CREATE TABLE IF NOT EXISTS recipe_ingredients (
            id          INTEGER PRIMARY KEY AUTOINCREMENT,
            recipe_id   INTEGER NOT NULL,
            item_id     INTEGER,
            item_name   TEXT NOT NULL,
            quantity    INTEGER NOT NULL DEFAULT 1,
            is_optional INTEGER NOT NULL DEFAULT 0,
            FOREIGN KEY (recipe_id) REFERENCES crafting_recipes(id),
            FOREIGN KEY (item_id) REFERENCES items(id)
        );

        CREATE TABLE IF NOT EXISTS meta (
            key   TEXT PRIMARY KEY,
            value TEXT
        );
        """;
}
