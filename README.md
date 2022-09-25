# PvPokeGMPatchTool, a.k.a. Grumpig

Grumpig is a tool to apply patches to the `gamemaster.json` file used by the offline version of [PvPoke](https://github.com/pvpoke/pvpoke).

## Installation

Download the binary from [Releases](https://github.com/StadiumGaming/PvPokeGMPatchTool/releases) or compile it yourself.

##### Windows:

Move the `Grumpig` folder found inside `Windows` to the `htdocs` folder in your xampp installation, most likely `C:/xampp/htdocs`. If you have no idea what that means, install PvPoke and its prerequisites first.

##### OSX / Linus:

Configure it yourself with the configuration options further down.

## Default functionality of the program

When Grumpig is ran, the following happen by default: 
* Checks for presence of the Patch File at the specified path (`patch.json` in the same folder as the executable by default), and loads it.
* Checks for presence of the Pure Gamemaster File (The vanilla gamemaster that the patch will be applied to), and creates it if not present with the method and location depending on the command line arguments passed.
* Loads the Pure Gamemaster File.
* Applies the changes specified in the Patch File to the loaded gamemaster.
* Saves the patched gamemaster to the Gamemaster File.
* Runs the server reboot script to restart the Apache server PvPoke runs on to put the changes into effect.


## Patch File Layout

The patch file is a JSON file with two array fields, `moves` and `pokemon`. Both of these are arrays of objects with three fields each, `action`, `target`, and `changes`, whose function depends on whether they're in `moves` or `pokemon`. An example patch file is provided at the end of this README.

#### Moves:

| Action | Target | Changes | Effect |
| --- | --- | --- | --- |
| `modify` | The move to modify. | JSON object containing all the fields of the move in the gamemaster file that are to be overwritten, and their new values. | Directly changes the stats of a move in the gamemaster without affecting its distribution.
| `clone` | The source move to clone. | JSON object containing all the fields of the cloned move in the gamemaster file that are to be overwritten, and their new values. | Creates a new move based on a preexisting one, alters its fields based on `changes`, and adds it to the movepools of all the Pokemon that know the source move. |
| `add` | Comma separated list of Pokemon to add the new move to. | JSON object containing a new move to be added to the gamemaster. | Adds a new move to the gamemaster, and adds it to the movepools of specified Pokemon. Their shadow versions will be automatically modified as well. |
| `delete` | The move to delete. | Unused, can be skipped. | Deletes a move from the gamemaster, all the cloned moves based off of it, and removes both base move and its clones from all pokemon movesets. If a Pokemon has a move in its default ranking moveset, deleting it from its moveset will have no effect. |

#### Pokemon:

| Action | Target | Changes | Effect |
| --- | --- | --- | --- |
| `modify` | The Pokemon to modify. | JSON object containing all the fields of the Pokemon in the gamemaster file that are to be overwritten, and their new values. | Directly changes the stats of a Pokemon in the gamemaster.
| `clone` | The source Pokemon to clone. | JSON object containing all the fields of the cloned Pokemon in the gamemaster file that are to be overwritten, and their new values. | Creates a new Pokemon based on a preexisting one and alters its fields based on `changes. |
| `add` | If equal to `"noshadow"`, a shadow version of the new Pokemon won't be automatically added. | JSON object containing a new Pokemon to be added to the gamemaster. | Adds a new Pokemon to the gamemaster, and unless explicitly disabled, its shadow version too. |
| `delete` | The ability to delete Pokemon is disabled for stability concerns. | Unused, can be skipped. | Does nothing. |
| `addmove` | The Pokemon whose movepool is to be added to. | JSON object with two optional fields: `fastMoves` is an array of fast move IDs to be added, `chargedMoves` is an array of charged move IDs to be added. | Adds specified moves to a Pokemon's moveset, together with all clones based on them. |
| `deletemove` | The Pokemon whose movepool is to be removed from. | JSON object with two optional fields: `fastMoves` is an array of fast move IDs to be removed, `chargedMoves` is an array of charged move IDs to be removed. | Removes specified moves from a Pokemon's moveset, together with all clones based on them. |


## Command Line Arguments

| Flag | Full Name | Default Value | Effect |
| --- | --- | --- | --- |
| `-q` | Quiet | `false` | Suppresses all non-error print statements. |
| `-v` | Verbose | `false` | Prints a lot and often. |
| `-r` | XAMPP Folder | `"pvpoke"` | If Grumpig installed in the default Windows location, can be set to access a different folder in `htdocs`, most useful if you have multiple versions of PvPoke installed at the same time. |
| `-f` | Patch File Path | `"./patch.json"` | Path to the Patch file. |
| `-g` | Gamemaster File | `"gamemaster.json"` | File name of the Gamemaster file in the Gamemaster folder. |
| `-G` | Gamemaster Folder | `../<XAMPP Folder>/src/data` | Path to the folder containing the Gamemaster file. |
| `-p` | Pure Gamemaster File | `"gamemaster_pure.json"` | File name of the Pure Gamemaster file in the Gamemaster file. Superseded by a custom Pure Gamemaster File Path. |
| `-P` | Pure Gamemaster File Path | `""` | Custom path to the Pure Gamemaster File. |
| `-d` | Download | `false` | If set to true, downloads the Pure Gamemaster from the PvPoke GitHub repo if not present. Otherwise, will create the Pure Gamemaster out of the already present Gamemaster File. |
| `-D` | Force Download | `false` | If set to true, downloads the Pure Gamemaster from the PvPoke GitHub repo, always. Don't do this unless you have a reason to. |
| `-n` | No Reset | `false` | If set to true, disables the server resetting script that normally runs after the patch is applied. |
| `-c` | Custom Reset Script Path | `""` | Path to the custom server resetting script to use instead of the default one after the patch is applied. |

## Example Patch File

```
{
	"moves": [{
			"action": "modify",
			"target": "AQUA_TAIL",
			"changes": {
				"power": 60
			}
		},
		{
			"action": "clone",
			"target": "POISON_FANG",
			"changes": {
				"power": 70
			}
		},
		{
			"action": "clone",
			"target": "COUNTER",
			"changes": {
				"power": 11
			}
		},
		{
			"action": "clone",
			"target": "CHARGE_BEAM",
			"changes": {
				"power": 111
			}
		},
		{
			"action": "add",
			"target": "altaria",
			"changes": {
				"moveId": "WEATHER_BALL_GUN",
				"name": "Weather Ball (Gun)",
				"type": "dragon",
				"power": 140,
				"energy": 35,
				"energyGain": 0,
				"cooldown": 500,
				"archetype": "Spam/Bait"
			}
		},
		{
			"action": "delete",
			"target": "CHARGE_BEAM"
		}
	],

	"pokemon": [{
			"action": "modify",
			"target": "shedinja",
			"changes": {
				"baseStats": {
					"atk": 153,
					"def": 73,
					"hp": 9001
				}
			}
		},
		{
			"action": "clone",
			"target": "pikachu",
			"changes": {
				"speciesName": "Pikablu",
				"speciesId": "pikablu",
				"types": ["electric", "water"]
			}
		},
		{
			"action": "add",
			"target": "noshadow",
			"changes": {
				"dex": 9001,
				"speciesName": "SSJ Goku (very epic)",
				"speciesId": "goku",
				"baseStats": {
					"atk": 9000,
					"def": 9000,
					"hp": 9000
				},
				"types": ["fighting", "none"],
				"fastMoves": ["COUNTER"],
				"chargedMoves": ["POWER_UP_PUNCH"],
				"defaultIVs": {
					"cp500": [7.5, 5, 7, 10],
					"cp1500": [21.5, 6, 15, 10],
					"cp2500": [41.5, 5, 15, 13],
					"cp2500l40": [38.5, 14, 14, 14]
				},
				"buddyDistance": 5,
				"thirdMoveCost": 75000,
				"released": false
			}
		},
		{
			"action": "delete",
			"target": "won't work"
		},
		{
			"action": "addmove",
			"target": "chansey",
			"changes": {
				"fastMoves": ["COUNTER"],
				"chargedMoves": ["FLAME_CHARGE", "EARTHQUAKE"]
			}
		},
		{
			"action": "deletemove",
			"target": "nidoqueen",
			"changes": {
				"chargedMoves": ["POISON_FANG"]
			}
		}
	]
}
```

