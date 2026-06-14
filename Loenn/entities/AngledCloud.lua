local drawableSpriteStruct = require("structs.drawable_sprite")

local AngledCloud = {}

AngledCloud.name = "BreadHelper/AngledCloud"
AngledCloud.depth = 0
AngledCloud.placements = {
    {
        name = "normal",
        data = {
            fragile = false,
            small = false,
            hasArrow = true,
            angle = 90.0
        }
    }
}

local normalScale = 1.0
local smallScale = 24 / 35

local function getTexture(entity)
    local fragile = entity.fragile

    if fragile then
        return "objects/clouds/fragile00"

    else
        return "objects/clouds/cloud00"
    end
end

function AngledCloud.sprite(room, entity)
    local texture = getTexture(entity)
    local sprite = drawableSpriteStruct.fromTexture(texture, entity)
    local small = entity.small
    local scale = small and smallScale or normalScale

    sprite:setScale(scale, 1.0)

    return sprite
end

return AngledCloud