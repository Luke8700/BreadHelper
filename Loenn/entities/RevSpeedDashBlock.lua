local dropdownOptions = {
  ["event:/game/general/wall_break_dirt"] = "event:/game/general/wall_break_dirt",
  ["event:/game/general/wall_break_ice"] = "event:/game/general/wall_break_ice",
  ["event:/game/general/wall_break_wood"] = "event:/game/general/wall_break_wood",
  ["event:/game/general/wall_break_stone"] = "event:/game/general/wall_break_stone",
}

local dropdownOptions2 = {
  ["FlipNone"] = "FlipNone",
  ["FlipX"] = "FlipX",
  ["FlipY"] = "FlipY",
  ["FlipBoth"] = "FlipBoth",
}

local fakeTilesHelper = require("helpers.fake_tiles")

local RevSpeedDashBlock = {}

RevSpeedDashBlock.name = "BreadHelper/RevSpeedDashBlock"
RevSpeedDashBlock.depth = 0

function RevSpeedDashBlock.placements()
    return {
        name = "RevSpeedDashBlock",
        data = {
            breakSound = "event:/game/general/wall_break_stone",
            tiletype = fakeTilesHelper.getPlacementMaterial(),
            HorizontalFlip = "FlipX",
            VerticalFlip = "FlipY",
            blendin = false,
            canDash = true,
            permanent = false,
            refillDash = false,
            refillSound = true,
            destroyAttached = true,
            width = 8,
            height = 8
        }
    }
end

RevSpeedDashBlock.fieldOrder = {
	"x", "y",
	"width", "height",
	"breakSound", "tiletype",
	"HorizontalFlip", "VerticalFlip",
	"blendin", "canDash", "permanent", "destroyAttached",
	"refillDash", "refillSound"
}

function RevSpeedDashBlock.fieldInformation()
  return {
    tiletype = {
      options = fakeTilesHelper.getTilesOptions(),
      editable = false
    },
    breakSound = {
      options = dropdownOptions,
      editable = true
    },
    HorizontalFlip = {
      options = dropdownOptions2,
      editable = false
    },
    VerticalFlip = {
      options = dropdownOptions2,
      editable = false
    },
    destroyAttached = {
      default = true
    }
  }
end

RevSpeedDashBlock.sprite = fakeTilesHelper.getEntitySpriteFunction("tiletype", "blendin")

return RevSpeedDashBlock

