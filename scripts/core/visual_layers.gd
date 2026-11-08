class_name VisualLayers
extends RefCounted
## Máscara de capas visuales (prioridad según documento del proyecto).

enum Layer {
	FIELD_VECTORS = 1 << 0,
	FIELD_MAGNITUDE = 1 << 1,
	SENSOR = 1 << 2,
	COMPONENTS_EX_EY = 1 << 3,
	POTENTIAL = 1 << 4,
	EQUIPOTENTIALS = 1 << 5,
	POTENTIAL_MAP = 1 << 6,
	FIELD_LINES = 1 << 7,
}

const DEFAULT_MASK: int = Layer.FIELD_VECTORS | Layer.SENSOR
