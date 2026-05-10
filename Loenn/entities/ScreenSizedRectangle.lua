local ScreenSizedRectangle = {}

ScreenSizedRectangle.name = "BreadHelper/ScreenSizedRectangle"
ScreenSizedRectangle.depth =-1000000
ScreenSizedRectangle.fillColor= {0.5, 0.8, 1.0, 0.3}
ScreenSizedRectangle.borderColor={1.0, 1.0, 1.0, 1.0}
ScreenSizedRectangle.placements = {
	name = "ScreenSizedRectangle",
}

function ScreenSizedRectangle.rectangle(room, entity)
	return utils.rectangle(entity.x, entity.y, 320, 180)
end

return ScreenSizedRectangle