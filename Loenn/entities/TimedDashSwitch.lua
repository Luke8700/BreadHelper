-- adapted from Lönn's src/entities/dash_switch.lua
-- Like 90% of this was copied from Snip tysm my goat :pray:
local dropdownOptions = {
  ["Default"] = "default",
  ["Mirror"] = "mirror",
}

local logging = require("logging")
local drawableSprite = require("structs.drawable_sprite")

local textures = {
    default = "objects/temple/dashButton00",
    mirror = "objects/temple/dashButtonMirror00",
}

local switchSides = {
  ["Down"] = 0,
  ["Up"] = 1,
  ["Right"] = 2,
  ["Left"] = 3
}

local sideToClockwiseRotations = {
  [0] = 0,
  [1] = 2,
  [2] = 3,
  [3] = 1
}
local clockwiseRotationsToSide = {
  [0] = 0,
  [1] = 3,
  [2] = 1,
  [3] = 2
}

local TimedDashSwitch = {}
TimedDashSwitch.name = "BreadHelper/TimedDashSwitch"
TimedDashSwitch.fieldInformation = {
  side = {
    options = switchSides,
    editable = false
  },
  sprite = {
    options = dropdownOptions,
    editable = true
  }
}

TimedDashSwitch.placements = {
  {
    name = "down",
    data = {
      side = 0,
      sprite = "default",
      allGates = false,
      openFast = false,
      flag = "",
      time = "2.0"
    }
  },
  {
    name = "left",
    data = {
      side = 3,
      sprite = "default",
      allGates = false,
      openFast = false,
      flag = "",
      time = "2.0"
    }
  },
  {
    name = "up",
    data = {
      side = 1,
      sprite = "default",
      allGates = false,
      openFast = false,
      flag = "",
      time = "2.0"
    }
  },
  {
    name = "right",
    data = {
      side = 2,
      sprite = "default",
      allGates = false,
      openFast = false,
      flag = "",
      time = "2.0"
    }
  }
}

function TimedDashSwitch.sprite(room, entity)
  local texture = entity.sprite == "default" and textures["default"] or textures["mirror"]
  local dashButtonSprite = drawableSprite.fromTexture(texture, entity)

  local side = entity.side
  if type(side) ~= "number" then
    return drawableSprite.fromInternalTexture("missing_image", entity)
  end

  if side == 0 then
    dashButtonSprite:addPosition(8, 0)
    dashButtonSprite.rotation = -math.pi / 2
  elseif side == 1 then
    dashButtonSprite:addPosition(8, 8)
    dashButtonSprite.rotation = math.pi / 2
  elseif side == 2 then
    dashButtonSprite:addPosition(0, 8)
    dashButtonSprite.rotation = math.pi
  elseif side == 3 then
    dashButtonSprite:addPosition(8, 8)
    dashButtonSprite.rotation = 0
  end

  return dashButtonSprite
end

function TimedDashSwitch.flip(room, entity, horizontal, vertical)
  local side = entity.side
  if type(side) ~= "number" then
    return false
  end

  if vertical then
    if side == 0 then
      entity.side = 1
      return true
    elseif side == 1 then
      entity.side = 0
      return true
    end
  elseif horizontal then
    if side == 2 then
      entity.side = 3
      return true
    elseif side == 3 then
      entity.side = 2
      return true
    end
  end
  return false
end

function TimedDashSwitch.rotate(room, entity, direction)
  local side = entity.side
  if not (type(side) == "number" and side >= 0 and side <= 3) then
    return false
  end

  local clockwiseRotations = sideToClockwiseRotations[side]

  -- lua's modulo operator behaves differently with negatives;
  -- (1 % 4 == 1) but (-1 % 4 == 3)
  -- this plays to our advantage here though, since 1 counterclockwise rotation
  -- (or -1 clockwise rotations) is the same as 3 clockwise rotations
  clockwiseRotations = (clockwiseRotations + direction) % 4

  entity.side = clockwiseRotationsToSide[clockwiseRotations]
  return true
end

function TimedDashSwitch.texture(room, entity)
  return entity.sprite
end

return TimedDashSwitch