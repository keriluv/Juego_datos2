class_name ExperimentManager
extends RefCounted

enum PresetId {
	NONE,
	SINGLE_POSITIVE,
	SINGLE_NEGATIVE,
	DIPOLE,
	TWO_EQUAL_POSITIVE,
	SYMMETRIC_DIPOLE,
}

const PRESET_LABELS: Dictionary = {
	PresetId.SINGLE_POSITIVE: "Una carga +Q",
	PresetId.SINGLE_NEGATIVE: "Una carga −Q",
	PresetId.DIPOLE: "Dipolo (+Q, −Q)",
	PresetId.TWO_EQUAL_POSITIVE: "Dos +Q (E=0 centro)",
	PresetId.SYMMETRIC_DIPOLE: "Dipolo (V=0 centro)",
}

## Casos rápidos Modo ExpoFísica (jurado).
const EXPO_PRESETS: Array = [
	PresetId.SINGLE_POSITIVE,
	PresetId.DIPOLE,
	PresetId.TWO_EQUAL_POSITIVE,
]

const EXPO_LABELS: Array = [
	"1 · Coulomb (una carga)",
	"2 · Superposición (dipolo)",
	"3 · Campo nulo (dos +Q)",
]


func apply_preset(preset: PresetId, simulation: Node) -> void:
	simulation.clear_charges()
	var q_nc: float = simulation.default_charge_nc
	match preset:
		PresetId.SINGLE_POSITIVE:
			simulation.add_charge(Charge.from_nanocoulombs(Vector2.ZERO, q_nc))
		PresetId.SINGLE_NEGATIVE:
			simulation.add_charge(Charge.from_nanocoulombs(Vector2.ZERO, -q_nc))
		PresetId.DIPOLE:
			simulation.add_charge(Charge.from_nanocoulombs(Vector2(-0.18, 0.0), q_nc))
			simulation.add_charge(Charge.from_nanocoulombs(Vector2(0.18, 0.0), -q_nc))
		PresetId.TWO_EQUAL_POSITIVE:
			simulation.add_charge(Charge.from_nanocoulombs(Vector2(-0.2, 0.0), q_nc))
			simulation.add_charge(Charge.from_nanocoulombs(Vector2(0.2, 0.0), q_nc))
		PresetId.SYMMETRIC_DIPOLE:
			simulation.add_charge(Charge.from_nanocoulombs(Vector2(-0.18, 0.0), q_nc))
			simulation.add_charge(Charge.from_nanocoulombs(Vector2(0.18, 0.0), -q_nc))
		_:
			pass
