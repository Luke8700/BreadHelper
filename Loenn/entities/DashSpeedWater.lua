local xnaColors = require("consts.xna_colors")
local lightBlue = xnaColors.LightBlue

local DashSpeedWater = {}

DashSpeedWater.name = "BreadHelper/DashSpeedWater"
DashSpeedWater.depth = 0
DashSpeedWater.fillColor = {lightBlue[1] * 0.3, lightBlue[2] * 0.3, lightBlue[3] * 0.3, 0.6}
DashSpeedWater.borderColor = {lightBlue[1] * 0.8, lightBlue[2] * 0.8, lightBlue[3] * 0.8, 0.8}
DashSpeedWater.placements = {
	name = "DashSpeedWater",
	data = {
		hasBottom = false,
		width = 8,
		height = 8,
		speedMod = 0.75
    }
}

return DashSpeedWater