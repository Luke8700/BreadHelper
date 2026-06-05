local dropdownOptions = {
  ["Reverse Speed"] = "ReverseSpeed",
  ["Eight Way"] = "8Way",
  ["Normal"] = "Normal",
}

local SpecialBumper = {}

SpecialBumper.name = "BreadHelper/SpecialBumper"
SpecialBumper.depth = 0
SpecialBumper.nodeLineRenderType = "line"
SpecialBumper.texture = "objects/Bumper/Idle22"
SpecialBumper.nodeLimits = {0, 1}
SpecialBumper.placements = {
        name = "SpecialBumper",
        data = {
            wobble = false,
            refillDash = true,
            launchMode = "8Way",
            dashCooldown = 0.2,
            respawnTime = 0.6,
            ignoreCoreMode = true
        }
}

function SpecialBumper.fieldInformation()
  return {
    launchMode = {
      options = dropdownOptions,
      editable = false
    }
  }
end

return SpecialBumper