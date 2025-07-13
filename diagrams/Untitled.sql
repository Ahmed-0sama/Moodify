CREATE TABLE [user] (
  [id] int PRIMARY KEY,
  [email] nvarchar(255) UNIQUE NOT NULL,
  [photo] varbinary
)
GO

CREATE TABLE [history] (
  [id] int PRIMARY KEY,
  [user_id] int,
  [music_id] int,
  [listened_at] datetime
)
GO

CREATE TABLE [favorites] (
  [userid] int,
  [musicid] int
)
GO

CREATE TABLE [music] (
  [music_id] int PRIMARY KEY,
  [count] int
)
GO

CREATE TABLE [artist] (
  [artist_id] int PRIMARY KEY,
  [artist_name] nvarchar(255)
)
GO

CREATE TABLE [artist_Music] (
  [artist_id] int,
  [music_id] int
)
GO

ALTER TABLE [history] ADD FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
GO

ALTER TABLE [history] ADD FOREIGN KEY ([music_id]) REFERENCES [music] ([music_id])
GO

ALTER TABLE [favorites] ADD FOREIGN KEY ([userid]) REFERENCES [user] ([id])
GO

ALTER TABLE [favorites] ADD FOREIGN KEY ([musicid]) REFERENCES [music] ([music_id])
GO

ALTER TABLE [artist_Music] ADD FOREIGN KEY ([artist_id]) REFERENCES [artist] ([artist_id])
GO

ALTER TABLE [artist_Music] ADD FOREIGN KEY ([music_id]) REFERENCES [music] ([music_id])
GO
