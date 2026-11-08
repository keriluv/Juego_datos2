extends Node
## Orquesta estado, caché del campo O(MN) y señales.

signal charges_changed
signal layers_changed(active_mask: int)
signal configuration_changed
signal field_cache_rebuilt

var coordinate_mapper: CoordinateMapper = CoordinateMapper.new()
var field_calculator: FieldCalculator = FieldCalculator.new()
var experiments: ExperimentManager = ExperimentManager.new()

var default_charge_nc: float = 10.0

var _charges: Array[Charge] = []
var _next_charge_id: int = 0
var _visual_layer_mask: int = VisualLayers.DEFAULT_MASK | VisualLayers.Layer.FIELD_MAGNITUDE

var _field_cache_valid: bool = false
var _cached_samples: Array = []
var _cached_grid_cols: int = 0
var _cached_grid_rows: int = 0
var _cache_charge_revision: int = 0
var _charge_revision: int = 0


func get_charges() -> Array[Charge]:
	return _charges


func get_visual_layer_mask() -> int:
	return _visual_layer_mask


func set_visual_layer_mask(mask: int) -> void:
	if _visual_layer_mask == mask:
		return
	_visual_layer_mask = mask
	layers_changed.emit(mask)


func toggle_layer(layer_flag: int, enabled: bool) -> void:
	var mask := _visual_layer_mask
	if enabled:
		mask |= layer_flag
	else:
		mask &= ~layer_flag
	set_visual_layer_mask(mask)


func is_layer_enabled(layer_flag: int) -> bool:
	return (_visual_layer_mask & layer_flag) != 0


func invalidate_field_cache() -> void:
	_field_cache_valid = false
	_charge_revision += 1
	configuration_changed.emit()


func is_field_cache_valid() -> bool:
	return _field_cache_valid


func get_cached_field_samples() -> Array:
	return _cached_samples


func sample_field_at(point_m: Vector2) -> FieldCalculator.FieldSample:
	return field_calculator.compute_field_at(point_m, _charges)


func ensure_field_cache(grid_cols: int, grid_rows: int) -> void:
	if (
		_field_cache_valid
		and grid_cols == _cached_grid_cols
		and grid_rows == _cached_grid_rows
	):
		return
	rebuild_field_cache(grid_cols, grid_rows)


func rebuild_field_cache(grid_cols: int, grid_rows: int) -> void:
	var he := coordinate_mapper.half_extent_m
	var bounds_min := Vector2(-he.x, -he.y)
	var bounds_max := Vector2(he.x, he.y)
	_cached_samples = field_calculator.compute_field_grid(
		bounds_min, bounds_max, grid_cols, grid_rows, _charges
	)
	_cached_grid_cols = grid_cols
	_cached_grid_rows = grid_rows
	_field_cache_valid = true
	_cache_charge_revision = _charge_revision
	field_cache_rebuilt.emit()


func clear_charges() -> void:
	_charges.clear()
	_next_charge_id = 0
	invalidate_field_cache()
	charges_changed.emit()


func add_charge(charge: Charge) -> void:
	charge.id = _next_charge_id
	_next_charge_id += 1
	_charges.append(charge)
	invalidate_field_cache()
	charges_changed.emit()


func remove_charge_by_id(charge_id: int) -> void:
	for i in _charges.size():
		if _charges[i].id == charge_id:
			_charges.remove_at(i)
			invalidate_field_cache()
			charges_changed.emit()
			return


func update_charge_position(charge_id: int, position_m: Vector2) -> void:
	for c in _charges:
		if c.id == charge_id:
			c.position_m = position_m
			invalidate_field_cache()
			charges_changed.emit()
			return


func update_charge_magnitude_nc(charge_id: int, q_nc: float) -> void:
	for c in _charges:
		if c.id == charge_id:
			c.charge_c = q_nc * PhysicsConstants.NANO_COULOMB_TO_COULOMB
			invalidate_field_cache()
			charges_changed.emit()
			return


func set_all_charges_magnitude_nc(q_nc: float) -> void:
	default_charge_nc = q_nc
	for c in _charges:
		c.charge_c = q_nc * PhysicsConstants.NANO_COULOMB_TO_COULOMB
	if not _charges.is_empty():
		invalidate_field_cache()
		charges_changed.emit()


func apply_experiment(preset: ExperimentManager.PresetId) -> void:
	experiments.apply_preset(preset, self)


func load_demo_default() -> void:
	clear_charges()
	apply_experiment(ExperimentManager.PresetId.SINGLE_POSITIVE)
