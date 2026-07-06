👾 Simulador de Ladrón - Juego 3D
Simulador de Ladrón es un videojuego 3D de sigilo y estrategia en primera persona. El jugador asume el papel de un ladrón que debe infiltrarse en la casa de su vecino, para robar todos los objetos que tiene en su lista, pero no puede permitir que le pillen.

🎮 Características principales
Mecánicas de Sigilo: Sistemas de detección basados en conos de visión, niveles de iluminación y propagación del sonido (ruido al caminar o tirar objetos).
Inteligencia Artificial (NPC): Vecino con máquina de estado (FSM) que patrulla, investiga ruidos sospechosos y persigue al jugador al detectarlo.
Sistema de Interacción Interacción con puertas y recolección de objetos mediante un minijuego básico.
🛠️ Tecnologías y Herramientas utilizadas
Motor de juego: [Unity]
Lenguaje de Programación: [C#]
Modelado 3D / Assets: [Assets de terceros usados]
Control de versiones: Git & GitHub
🕹️ Controles Básicos
| Acción | Teclado / Ratón |

| :--- | :--- |

| Movimiento | W A S D |

| Cámara | Ratón |

| Interactuar / Robar | E |

| Agacharse (Sigilo) | Ctrl |

| Correr | Shift |

| Inventario | Tab |

🧠 Retos Técnicos y Aprendizaje
Inteligencia Artificial: Programación de una Máquina de Estados Finitos (FSM) para el vecino, gestionando los estados Patrullar, Sospechar, Buscar y Perseguir. Integración con NavMesh para el trazado de rutas esquivando obstáculos.
Sistema de Detección (Raycasting): Implementación de lógicas matemáticas y Raycasts para calcular si el jugador está dentro del campo visual del enemigo y si hay paredes bloqueando la vista.
