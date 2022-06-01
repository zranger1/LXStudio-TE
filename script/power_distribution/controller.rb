require 'csv'
require './calculate_line_lengths'
require './constants'
require './junction_box'

class Controller
  @@vertex_counter = {}

  def initialize(vertex:)
    @vertex = vertex
    @id = calculate_id
    @junction_box = nil
    @panels = []
    # Note: only covers the "injection" edge of the signal run, not any edges chained from there.
    @edges = []
  end

  def decorated_id
    "C#{@id}"
  end

  def self.load_controllers(edge_signal_filename:, panel_signal_filename:, graph:, vertices:)
    controllers = {}
    populate_edge_controllers(filename: edge_signal_filename, controllers: controllers, graph: graph, vertices: vertices)
    populate_panel_controllers(filename: panel_signal_filename, controllers: controllers, graph: graph, vertices: vertices)

    total_controllers = 0
    controllers.each do |_, assigned_controllers_at_vertex|
      total_controllers += assigned_controllers_at_vertex.length
    end

    if total_controllers != EXPECTED_TOTAL_CONTROLLER_COUNT
      raise "loaded #{total_controllers} controllers; expected #{EXPECTED_TOTAL_CONTROLLER_COUNT}"
    end

    controllers
  end

  def self.assign_controllers_to_boxes(graph:, controllers:, junction_boxes:)
    controllers.each do |controller_vertex_id, vertex_controllers|
      vertex_controllers.each do |controller|
        shortest_eligible_distance_to_box_from_controller = 999999
        nearest_eligible_box = nil

        junction_boxes.each do |_, boxes|
          # Multiple boxes may be at each vertex depending upon nearby power needs.
          boxes.each do |box|
            if box.controllers.length == MAX_CONTROLLERS_PER_JUNCTION_BOX
              next
            end

            min_distance = min_distance_between_vertices_in_feet(graph, controller.vertex.id, box.vertex.id)
            if min_distance < shortest_eligible_distance_to_box_from_controller
              shortest_eligible_distance_to_box_from_controller = min_distance
              nearest_eligible_box = box

              # No need to calculate for other boxes; they're at the same distance.
              next
            end
          end
        end
        nearest_eligible_box.assign_controller(controller)
      end
    end
  end

  def assign_signal_to_edge(edge:)
    if panels.length + edges.length >= MAX_CHANNELS_PER_CONTROLLER
      raise 'assigned too many signal runs already'
    end
  end

  def assign_signal_to_panel(panel:)
    if panels.length + edges.length >= MAX_CHANNELS_PER_CONTROLLER
      raise 'assigned too many signal runs already'
    end
  end

  attr_accessor :edges, :panels, :junction_box, :vertex, :id

  private

  def calculate_id
    counter = @@vertex_counter[vertex.id]
    if counter.nil?
      @@vertex_counter[vertex.id] = 0
      counter = 0
    end
    id = "#{vertex.id}-#{counter}"
    @@vertex_counter[vertex.id] += 1

    id
  end

  def self.assign_new_controller_at_vertex(vertex:, controllers:)
      # Controllers are identified with `vertex-number_at_vertex`. e.g. the second controller
      # at vertex 100 will be 100-1.
      controller = Controller.new(vertex: vertex)
      if controllers[vertex.id] != nil
        controllers[vertex.id].push(controller)
      else
        controllers[vertex.id] = [controller]
      end
  end

  def self.populate_edge_controllers(filename:, controllers:, graph:, vertices:)
    rows = CSV.read(filename, col_sep: "\t")

    rows.drop(1).each do |row|
      _, signal_from, controller_vertex_id = row

      if signal_from != 'Controller'
        next
      end

      controller_vertex = vertices.find { |vertex_id, vertex| vertex_id.to_s == controller_vertex_id }[1]

      assign_new_controller_at_vertex(vertex: controller_vertex, controllers: controllers)
    end
    controllers
  end

  def self.populate_panel_controllers(filename:, controllers:, graph:, vertices:)
    rows = CSV.read(filename, col_sep: "\t")

    rows.drop(1).each do |row|
      _, _, _, controller_vertex_id = row

      controller_vertex = vertices.find { |vertex_id, vertex| vertex_id.to_s == controller_vertex_id }[1]

      assign_new_controller_at_vertex(vertex: controller_vertex, controllers: controllers)
    end
    controllers
  end
end
