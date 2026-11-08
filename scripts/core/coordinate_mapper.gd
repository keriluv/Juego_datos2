class_name CoordinateMapper
extends RefCounted
## Convierte entre coordenadas de pantalla (px) y físicas (m).
## El origen del mundo físico coincide con el centro del área de laboratorio.

var viewport_size_px: Vector2 = Vector2(800.0, 600.0)
## Semiancho y semialto del dominio físico visible (m) desde el origen.
var half_extent_m: Vector2 = Vector2(0.5, 0.375)


func set_viewport_size(size_px: Vector2) -> void:
	viewport_size_px = size_px


func set_half_extent(half_x_m: float, half_y_m: float) -> void:
	half_extent_m = Vector2(half_x_m, half_y_m)


func meters_to_pixels(point_m: Vector2) -> Vector2:
	if half_extent_m.x <= 0.0 or half_extent_m.y <= 0.0:
		return Vector2.ZERO
	var normalized := Vector2(
		point_m.x / half_extent_m.x,
		-point_m.y / half_extent_m.y
	)
	return Vector2(
		(normalized.x + 1.0) * 0.5 * viewport_size_px.x,
		(normalized.y + 1.0) * 0.5 * viewport_size_px.y
	)


func pixels_to_meters(point_px: Vector2) -> Vector2:
	if viewport_size_px.x <= 0.0 or viewport_size_px.y <= 0.0:
		return Vector2.ZERO
	var normalized := Vector2(
		(point_px.x / viewport_size_px.x) * 2.0 - 1.0,
		(point_px.y / viewport_size_px.y) * 2.0 - 1.0
	)
	return Vector2(
		normalized.x * half_extent_m.x,
		-normalized.y * half_extent_m.y
	)
