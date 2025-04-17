import bpy
import bmesh

for obj in bpy.context.selected_objects:
    if obj.type == "MESH":
        #remove any existing color attributes so that we're working with one consistent type 
        if obj.data.color_attributes: 
            obj.data.color_attributes.remove(obj.data.color_attributes[0])

        bpy.ops.geometry.color_attribute_add(name = 'Color', domain = 'CORNER', data_type = 'FLOAT_COLOR')
             
        bm = bmesh.new()
        bm.from_mesh(obj.data)  
        col_layer = bm.loops.layers.float_color[0]

        for face in bm.faces:
            for vert in face.verts:
                for loop in vert.link_loops:
                    rescaled_color = [(vert.normal[0]/2)+0.5,(vert.normal[1]/2)+0.5,(vert.normal[2]/2)+0.5]
                    loop[col_layer] = (rescaled_color[0], rescaled_color[1],rescaled_color[2],1) 
            
        #write back to original object        
        bm.to_mesh(obj.data)
        
        

