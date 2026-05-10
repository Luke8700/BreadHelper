local FlagOnBerryPickupController = {}

FlagOnBerryPickupController.name = "BreadHelper/FlagOnBerryPickupController"
FlagOnBerryPickupController.depth = 0
FlagOnBerryPickupController.texture = "loenn/BreadHelper/FlagPickupBerryController"
FlagOnBerryPickupController.placements = {
	name = "FlagOnBerryPickupController",
	data = {
		flagName = "BerryPickup",
		checkGolden = false,
		checkMoon = true
	}
}

return FlagOnBerryPickupController