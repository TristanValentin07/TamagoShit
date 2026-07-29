import rhinoscriptsyntax as rs
import math

# --- FONCTION UTILITAIRE ---
def create_hexagon(plane, radius):
    """Génère une polyligne fermée à 6 côtés (hexagone)."""
    points = []
    for i in range(7):
        angle = i * (math.pi / 3.0)
        x = radius * math.cos(angle)
        y = radius * math.sin(angle)
        pt = rs.PlaneEvaluate(plane, x, y)
        points.append(pt)
    return rs.AddPolyline(points)

# --- LES 15 PIÈCES MÉCANIQUES ---

def create_gear(center, radius, thickness, teeth_count):
    plane = rs.MovePlane(rs.WorldXYPlane(), center)
    gear_parts = []
    body = rs.AddCylinder(plane, thickness, radius)
    if body: gear_parts.append(body)
    
    tooth_radius = radius * 0.15
    for i in range(teeth_count):
        angle = i * (2 * math.pi / teeth_count)
        x = center[0] + radius * math.cos(angle)
        y = center[1] + radius * math.sin(angle)
        tooth_plane = rs.MovePlane(rs.WorldXYPlane(), [x, y, center[2]])
        tooth = rs.AddCylinder(tooth_plane, thickness, tooth_radius)
        if tooth: gear_parts.append(tooth)
        
    hole = rs.AddCylinder(plane, thickness * 1.1, radius * 0.3)
    if hole and body: rs.BooleanDifference(gear_parts, [hole], delete_input=True)

def create_piston(center):
    plane = rs.MovePlane(rs.WorldXYPlane(), center)
    rs.AddCylinder(plane, 15.0, 4.0)
    rod_plane = rs.MovePlane(plane, [0, 0, 15.0])
    rs.AddCylinder(rod_plane, 10.0, 1.5)
    ring_plane = rs.MovePlane(rs.WorldZXPlane(), [center[0], center[1], center[2] + 25.0])
    ring_circle = rs.AddCircle(ring_plane, 2.5)
    rs.AddPipe(ring_circle, 0, 0.8)
    rs.DeleteObject(ring_circle)

def create_spring(center):
    pt1 = center
    pt2 = [center[0], center[1], center[2] + 20.0]
    spiral = rs.AddSpiral(pt1, pt2, 3.5, 3.5, 20.0 / 3.0)
    if spiral:
        rs.AddPipe(spiral, 0, 0.6)
        rs.DeleteObject(spiral)

def create_flanged_pipe(center):
    plane = rs.MovePlane(rs.WorldXYPlane(), center)
    rs.AddCylinder(plane, 20.0, 2.5)
    rs.AddCylinder(plane, 1.5, 4.5)
    rs.AddCylinder(rs.MovePlane(plane, [0, 0, 18.5]), 1.5, 4.5)

def create_nut_and_bolt(center):
    plane = rs.MovePlane(rs.WorldXYPlane(), center)
    hex_head = create_hexagon(plane, 3.5) 
    head_path = rs.AddLine(center, [center[0], center[1], center[2] + 2.0])
    head_extrusion = rs.ExtrudeCurve(hex_head, head_path)
    if head_extrusion: rs.CapPlanarHoles(head_extrusion)
    
    rs.AddCylinder(rs.MovePlane(plane, [0, 0, 2.0]), 15.0, 1.5)
    
    nut_z = center[2] + 10.0
    nut_plane = rs.MovePlane(plane, [0, 0, nut_z])
    nut_hex = create_hexagon(nut_plane, 4.0)
    nut_path = rs.AddLine([center[0], center[1], nut_z], [center[0], center[1], nut_z + 2.5])
    nut_extrusion = rs.ExtrudeCurve(nut_hex, nut_path)
    if nut_extrusion: rs.CapPlanarHoles(nut_extrusion)
    
    rs.DeleteObjects([hex_head, head_path, nut_hex, nut_path])

def create_pulley(center):
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), center), 2.0, 8.0)
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [center[0], center[1], center[2]+2.0]), 2.0, 6.0)
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [center[0], center[1], center[2]+4.0]), 2.0, 8.0)

def create_ball_bearing(center):
    plane = rs.MovePlane(rs.WorldXYPlane(), center)
    outer = rs.AddCylinder(plane, 4.0, 10.0)
    outer_hole = rs.AddCylinder(plane, 4.5, 8.0)
    if outer and outer_hole: rs.BooleanDifference([outer], [outer_hole], delete_input=True)
    
    inner = rs.AddCylinder(plane, 4.0, 5.0)
    inner_hole = rs.AddCylinder(plane, 4.5, 3.0)
    if inner and inner_hole: rs.BooleanDifference([inner], [inner_hole], delete_input=True)
    
    for i in range(8):
        angle = i * (2 * math.pi / 8)
        x = center[0] + 6.5 * math.cos(angle)
        y = center[1] + 6.5 * math.sin(angle)
        rs.AddSphere([x, y, center[2] + 2.0], 1.4)

def create_valve_wheel(center):
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), center), 3.0, 2.5)
    rim_plane = rs.MovePlane(rs.WorldXYPlane(), [center[0], center[1], center[2]+1.5])
    rs.AddTorus(rim_plane, 8.0, 1.2)
    for i in range(5):
        angle = i * (2 * math.pi / 5)
        end_pt = [center[0] + 8.0 * math.cos(angle), center[1] + 8.0 * math.sin(angle), center[2]+1.5]
        line = rs.AddLine([center[0], center[1], center[2]+1.5], end_pt)
        rs.AddPipe(line, 0, 0.8)
        rs.DeleteObject(line)

def create_crankshaft(center):
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), center), 5.0, 2.0)
    web1 = rs.MovePlane(rs.WorldYZPlane(), [center[0]-3.0, center[1], center[2]+5.0])
    rs.AddCylinder(web1, 6.0, 1.5)
    offset_plane = rs.MovePlane(rs.WorldXYPlane(), [center[0]+3.0, center[1], center[2]+6.5])
    rs.AddCylinder(offset_plane, 5.0, 2.0)
    web2 = rs.MovePlane(rs.WorldYZPlane(), [center[0]-3.0, center[1], center[2]+11.5])
    rs.AddCylinder(web2, 6.0, 1.5)
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [center[0], center[1], center[2]+13.0]), 5.0, 2.0)

def create_chain_link(center):
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), center), 8.0, 1.5)
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [center[0]+7.0, center[1], center[2]]), 8.0, 1.5)
    p1 = rs.MovePlane(rs.WorldYZPlane(), [center[0]-1.5, center[1], center[2]+1.0])
    rs.AddCylinder(p1, 10.0, 2.2)
    p2 = rs.MovePlane(rs.WorldYZPlane(), [center[0]-1.5, center[1], center[2]+7.0])
    rs.AddCylinder(p2, 10.0, 2.2)

def create_universal_joint_cross(center):
    p_x = rs.MovePlane(rs.WorldYZPlane(), [center[0]-6.0, center[1], center[2]+3.0])
    rs.AddCylinder(p_x, 12.0, 1.8)
    p_y = rs.MovePlane(rs.WorldZXPlane(), [center[0], center[1]-6.0, center[2]+3.0])
    rs.AddCylinder(p_y, 12.0, 1.8)
    rs.AddSphere([center[0], center[1], center[2]+3.0], 2.5)

def create_wingnut(center):
    plane = rs.MovePlane(rs.WorldXYPlane(), center)
    rs.AddCylinder(plane, 3.0, 3.0)
    w1_plane = rs.MovePlane(rs.WorldYZPlane(), [center[0]+2.0, center[1], center[2]+1.0])
    w1_plane = rs.RotatePlane(w1_plane, 45, w1_plane.YAxis)
    rs.AddCylinder(w1_plane, 5.0, 1.0)
    w2_plane = rs.MovePlane(rs.WorldYZPlane(), [center[0]-2.0, center[1], center[2]+1.0])
    w2_plane = rs.RotatePlane(w2_plane, -45, w2_plane.YAxis)
    w2_plane = rs.MovePlane(w2_plane, rs.VectorScale(w2_plane.ZAxis, -5.0))
    rs.AddCylinder(w2_plane, 5.0, 1.0)

def create_propeller(center):
    rs.AddSphere(rs.MovePlane(rs.WorldXYPlane(), [center[0], center[1], center[2]+3.0]).Origin, 3.0)
    for i in range(3):
        angle = i * (2 * math.pi / 3)
        blade_plane = rs.MovePlane(rs.WorldZXPlane(), [center[0], center[1], center[2]+3.0])
        blade_plane = rs.RotatePlane(blade_plane, math.degrees(angle), rs.WorldXYPlane().ZAxis)
        blade_plane = rs.RotatePlane(blade_plane, 30, blade_plane.XAxis)
        rs.AddCylinder(blade_plane, 10.0, 1.0)

def create_sprocket(center):
    plane = rs.MovePlane(rs.WorldXYPlane(), center)
    rs.AddCylinder(plane, 2.0, 8.0)
    rs.AddCylinder(plane, 2.5, 4.0)
    for i in range(16):
        angle = i * (2 * math.pi / 16)
        x = center[0] + 8.0 * math.cos(angle)
        y = center[1] + 8.0 * math.sin(angle)
        cone_plane = rs.MovePlane(rs.WorldXYPlane(), [x, y, center[2]])
        rs.AddCylinder(cone_plane, 2.0, 1.2)

def create_pressure_gauge(center):
    plane = rs.MovePlane(rs.WorldXYPlane(), center)
    rs.AddCylinder(plane, 4.0, 5.0)
    face_plane = rs.MovePlane(rs.WorldXYPlane(), [center[0], center[1], center[2]+4.0])
    rs.AddCylinder(face_plane, 0.5, 4.5)
    rs.AddCylinder(rs.MovePlane(rs.WorldXYPlane(), [center[0], center[1], center[2]+4.5]), 1.0, 0.5)
    needle = rs.AddLine([center[0], center[1], center[2]+4.6], [center[0]+3.0, center[1]+2.0, center[2]+4.6])
    rs.AddPipe(needle, 0, 0.3)
    rs.DeleteObject(needle)


# --- FONCTION PRINCIPALE MODIFIÉE POUR DISPOSITION EN GRILLE ---

def generate_all_mechanical_parts():
    rs.EnableRedraw(False)
    
    # Paramètres de la grille
    spacing_x = 30.0  # Espace entre les colonnes
    spacing_y = 30.0  # Espace entre les lignes
    columns_count = 5 # Nombre maximal d'éléments par ligne
    
    parts_functions = [
        create_gear, create_piston, create_spring, create_flanged_pipe, create_nut_and_bolt,
        create_pulley, create_ball_bearing, create_valve_wheel, create_crankshaft, create_chain_link,
        create_universal_joint_cross, create_wingnut, create_propeller, create_sprocket, create_pressure_gauge
    ]
    
    for idx, func in enumerate(parts_functions):
        # Calcul de la rangée (row) et de la colonne (col) pour chaque index
        row = idx // columns_count
        col = idx % columns_count
        
        # Définition des coordonnées dynamiques de la grille
        current_x = col * spacing_x
        current_y = row * spacing_y
        
        try:
            if func == create_gear:
                func([current_x, current_y, 0], radius=8.0, thickness=2.0, teeth_count=12)
            else:
                func([current_x, current_y, 0])
        except Exception as e:
            print("Erreur sur la pièce index {}: {}".format(idx, e))
        
    rs.EnableRedraw(True)
    print("Génération de la grille (5 colonnes x 3 lignes) terminée avec succès !")

if __name__ == "__main__":
    generate_all_mechanical_parts()