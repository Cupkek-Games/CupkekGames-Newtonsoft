using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace CupkekGames.Newtonsoft
{
    public class SaveMigratorBuilder
    {
        private readonly Dictionary<int, Action<JObject>> _migrations = new();
        private int _currentVersion = 1;

        public SaveMigratorBuilder(int version)
        {
            _currentVersion = version;
        }

        public SaveMigratorBuilder AddMigration(int fromVersion, Action<JObject> migration)
        {
            if (fromVersion >= _currentVersion)
            {
                throw new ArgumentException(
                    $"Migration version ({fromVersion}) must be less than current version ({_currentVersion})"
                );
            }

            _migrations.Add(fromVersion, migration);
            return this;
        }

        public SaveMigrator Build()
        {
            // Verify we have all needed migrations
            for (int version = 1; version < _currentVersion; version++)
            {
                if (!_migrations.ContainsKey(version))
                {
                    throw new InvalidOperationException(
                        $"Missing migration for version {version}. All versions from 1 to {_currentVersion - 1} must have migrations."
                    );
                }
            }

            return new SaveMigrator(_currentVersion, _migrations);
        }
    }
}