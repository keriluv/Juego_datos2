class_name PhysicsConstants
extends RefCounted
## Constantes SI y parámetros numéricos del simulador.
## Fase 1: definición; el motor físico se implementa en Fase 2.

## Constante de Coulomb (N·m²/C²)
const K_COULOMB: float = 8.9875517923e9

## Conversión de carga introducida por el usuario (nC → C)
const NANO_COULOMB_TO_COULOMB: float = 1.0e-9

## Distancia mínima |r| para evaluar E y V cerca de una carga puntual (m).
## Significado físico: una carga puntual ideal concentra todo el campo en r=0;
## el modelo numérico excluye una vecindad infinitesimal para evitar división por cero.
## Los valores dentro de este radio no representan la singularidad real; se documentan
## en telemetría cuando el sensor entra en la región excluida (Fase 4+).
const MIN_DISTANCE_M: float = 1.0e-3
