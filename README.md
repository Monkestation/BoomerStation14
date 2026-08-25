<p align="center"> <img alt="Boomerstation Logo" width="880" height="200" src="https://github.com/Monkestation/BoomerStation14/blob/master/Resources/Textures/Logo/logo.png" /></p>

This is BoomerStation, a fork of the Funky Station. To prevent people forking RobustToolbox, a "content" pack is loaded by the client and server. This content pack contains everything needed to play the game on one specific server.

If you want to host or create content for SS14, or for BoomerStation, go to the [Space Station 14 repository](https://github.com/space-wizards/space-station-14), or the [BoomerStation repository](https://github.com/BoomerStation14/BoomerStation14).

## Links

[Monkestation Discord Server](https://discord.com/invite/monkestation)

## Documentation/Wiki

The [Funky Station Developer Documentation](https://docs.funkystation.org/) has information on how to contribute to Funky Station for now, until we get our own set up. It contains guides, game design documents and helpful tips on how to contribute to a repository.

## Contributing

We welcome everyone to contribute to our fork. Please join our Discord for collaborating!
We recommend you read the contribution guidelines. [Contribution Guidelines](https://docs.spacestation14.com/en/general-development/codebase-info/pull-request-guidelines.html)

## Building

1. Clone this repo:
```shell
git clone https://github.com/Monkestation/BoomerStation14.git
```
2. Go to the project folder and run `RUN_THIS.py` to initialize the submodules and load the engine:
```shell
cd BoomerStation14
python RUN_THIS.py
```
3. Compile the solution:

Build the server using `dotnet build`.

[More detailed instructions on building the project.](https://docs.spacestation14.com/en/general-development/setup.html)

## License

This repository is MIT. See `LICENSES` for a copy of the MIT license.

Most media assets are licensed under [CC-BY-SA 3.0](https://creativecommons.org/licenses/by-sa/3.0/) unless stated otherwise. Assets have their license and the copyright in the metadata file. [Example](https://github.com/space-wizards/space-station-14/blob/master/Resources/Textures/Objects/Tools/crowbar.rsi/meta.json).

If you find that your work is misattributed or someone elses work is misattributed, please create an issue on this repo's GitHub page, or contact us on the [Monkestation Discord Server](https://discord.com/invite/monkestation).

## Attributions and Namespaces

Our folders are modularized to avoid merge conflicts down the line. Content found within these subdirectories either originate directly from the source or are edited to fit Boomerstation's needs.

Boomer Station is MIT, therefore it cannot accept content from AGPL (or any other incompatible licenses) sources, unless the content has a dual license or has been explicitely relicensed to MIT by the author. PRs porting content from AGPL forks **MUST INCLUDE PROOF OF RELICENSING/DUAL LICENSING** in their PRs.

| Subdirectory     | Fork Name           | Fork Repository                                         | License  |
|------------------|---------------------|---------------------------------------------------------|----------|
| `_Monkestation`  | Boomer Station      | https://github.com/Monkestation/BoomerStation14         | MIT      |
| `_Funkystation`  | Funky Station       | https://github.com/funky-station/forky-station          | MIT      |
| `_MACRO`         | Macrocosm           | https://github.com/syndicate-ss14/macrocosm             | MIT      |
| `_Starfall`      | Starfall Drift      | https://github.com/Starfall-Drift/Starfall-Drift        | MIT      |
| `_CD`            | Cosmatic Drift      | https://github.com/cosmatic-drift-14/cosmatic-drift     | MIT      |
| `_Starlight`     | Starlight           | https://github.com/ss14Starlight/space-station-14       | MIT, Starlight License|
| `_Umbra`         | Sector Umbra        | https://github.com/Sector-Umbra/Sector-Umbra            | MIT      |
| `_Carpmosia`     | Carpmosia           | https://github.com/carpmosia/carpmosia                  | MIT      |

Additional repos that we have ported features from without subdirectories are listed below.
| Fork Name | Fork Repository | License |
|-----------|-----------------|---------|
| Space Station 14 | https://github.com/space-wizards/space-station-14 | MIT |
