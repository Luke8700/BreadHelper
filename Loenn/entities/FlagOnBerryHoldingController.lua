local FlagOnBerryHoldingController = {}

FlagOnBerryHoldingController.name = "BreadHelper/FlagOnBerryHoldingController"
FlagOnBerryHoldingController.depth = 0
FlagOnBerryHoldingController.texture = "loenn/BreadHelper/FlagHoldingBerryController"
FlagOnBerryHoldingController.placements = {
	name = "FlagOnBerryHoldingController",
	data = {
		flagName = "IsHoldingBerry",
		checkGolden = false,
		checkMoon = true
	}
}

return FlagOnBerryHoldingController