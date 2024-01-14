# Augmented Enhancer Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed

- 'Unlock Suits' option replaced with 'Free Unlockables'

### Added

- Configurable list of unlockables to unlock at game start
- 24 hour clock option
- Options to choose where the clock is hidden/visible
- Options to configure the saved item quantity cap
- `lights` command group
- Option to only overwrite challenge moon leaderboard entry when your score is higher
  than your previous best attempt

### Fixed

### Removed

## [0.5.0]

> [!WARNING]
> Contains breaking changes!

### Changed

- All patches now have their own ManualLogSource
- UnlockSuits patch refactored to be name-dependent (as opposed to index-dependent)
- AlwaysShowTerminal patch updated to use a transpiler
  - Scroll will no longer reset when exiting the terminal
- Some log-messages that were previously on the `Info` channel have been moved to `Debug`
- Ran JetBrains formatter on all project sources
- AlwaysShowTerminal uses transpilers; prevents automatic 'help' command upon opening terminal + prevents scroll reset
- All config options have been renamed / given sections
- Use TerminalApi to register threat scan as its own command
- Creates a persistent GameObject to apply patches

### Added

- 'Interval' config type + serializer
- Quota increase exponent option
- Scrap tweaks
  - Scrap spawns scalar
  - Scrap value scalar
  - Scrap 'playercount scaling' - more players, more scrap (with less value per item)
- Deadline duration tweaks
  - Essentially integrate DynamicDeadline features

### Fixed

- Patcher now ensures nested types are also patched

### Removed

## [0.4.1] 2023-12-31

### Fixed

- Publish workflow was not using tcli from local toolcache, so failed

## [0.4.0] 2023-12-31

### Changed

- Target framework is netstandard2
- Build/publish workflows use MinVer to determine target version
- `bDaysPerQuotaEnabled` renamed to `bDaysPerQuotaAssignmentEnabled`
- `iQuotaDays` renamed to `iQuotaAssignmentDays`
- All config options are now sectioned tidily

### Added

- Dynamic patching - Patches can be enabled/disabled while the game is running (using in-game config editor from another mod)
- Patch lifecycle methods
- Scrap value/quantity scalars
- 'Fairness Scaling' - increases number of scrap items but decreases scrap value w/ more players present

### Removed

- 'build' project

## [0.3.0] - 2023-12-06

### Added

- Passive income options

## [0.2.0] - 2023-12-05

### Changed

- Features' patch-classes have been separated
- Renamed various configuration options
- Renamed various classes
- Threat scanner has been refactored and is configured by an Enum, instead of an int
- Scrap protection is achieved by a transpiler
- Scrap protection configurable is now a continuous variable, rather than discrete enum

### Added

- Scrap protection 'randomness' configuration
- More quota configuration options:
  - starting credits
  - starting quota
  - quota increase steepness
  - quota base increase
  - quota increase randomness
- Feature flags for most options
- Delegation: features will be automatically delegated to other mods (disabled) depending on what is installed
  - Delegating most features to [Lethal Enhancer](https://thunderstore.io/c/lethal-company/p/Mom_Llama/Lethal_Company_Enhancer/)
  - Delegating days per quota to [Dynamic Deadline](https://thunderstore.io/c/lethal-company/p/Krayken/DynamicDeadline/)
- Feature flag for disabling delegation
- Configurable death penalty options

### Fixed

- The 'release' workflow was failing when there was no changelog to commit
- Scrap protection attempting to remove items

## [0.1.3] - 2023-12-04

### Fixed

- Removed categories from `thunderstore.toml` as `tcli` has not released the necessary feature

## [0.1.2] - 2023-12-04

### Fixed

- Specified categories in `thunderstore.toml` on a per-community basis to prevent `HTTP 400` on publish

## [0.1.1] - 2023-12-04

### Changed

- Build task ensures release tags begin with `v` and uses the remaining substring as the version

### Fixed

- Publish workflow contained syntax errors

## [0.1.0] - 2023-12-04

### Changed

- Forked from https://github.com/Crunchepillar/Lethal-Company-Enhancer/
- Project restructured
- Project renamed
- Project icon replaced

### Added

- CI/CD build setup
- Automatic publish to Thunderstore via GitHub actions

## [0.0.5] - 2023-11-21

### Fixed

- Scrap protection no long breaks things when failing a quota (Bug Smashers: Pinny/Toast)
- Improved compatability with Bigger Lobby 2.2.2+ (Bug Annihilator: Bizzle)

## [0.0.4] - 2023-11-18

### Added

- Added Dat1Mew's lovely icon to Thunderstore release

### Fixed

- Scrap Protection mode COINFLIP bug fixed to actually flip a coin (Bug Bonker: Vasanex)
- RPC added to properly inform clients of the company price each day

## [0.0.3] - 2023-11-17

### Added

- `eScrapProtection` configured value: has a few simple options for protecting scrap when the party wipes

## [0.0.2] - 2023-11-15

### Changed

- Company buy prices are derived from level data so they stay they same after save/load
- Plugin moved to net472 to fix dependency errors
- Project updated to make compiling smoother

### Added

- `bUnlockSuits` configured value: Add Green and Hazard suit to ship

## [0.0.1] - 2023-11-13

### Added

- `bEnabled` configured value: global toggle
- `bAlwaysShowTerminal` configured value: show terminal without players
- `bUseRandomPrices` configured value: randomly modifies company prices
- `fTimeScale` configured value: modifies time on moons
- `fMinCompanyBuyPCT` configured value: sets a floor for company prices
- `fDoorTimer` configured value: modifies how long the hangar doors remain closed
- `iQuotaDays` configured value: modifies how many days the players have to meet quota
- `eThreatScannerType` configured value: adds a threat scanner to "scan" of specified type
