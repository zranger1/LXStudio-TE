require 'csv'
require './constants.rb'

class Edge
  # 60 / m LED strips
  LEDS_PER_MICRON = 0.00006

  STRIPS_PER_EDGE = 3

  def initialize(id:, vertices:)
    @id = id
    @vertices = vertices
    @signal_to = nil
    @signal_from = nil
    @strips = Array.new(STRIPS_PER_EDGE) { |i| EdgeStrip.new(id: "#{id}-#{i}",vertices: vertices, edge_id: id) }
  end

  def length
    @length ||= vertices[1].distance(vertices[0])
  end

  def num_leds
    @num_leds ||= strips.sum(&:num_leds)
  end

  def max_current
    @max_current ||= strips.sum(&:max_current)
  end

  def self.load_edges(filename, vertices)
    rows = CSV.read(filename, col_sep: "\t")
    edges = {}
    rows.each do |row|
      vs = row[0].split('-').map { |v| vertices[v.to_i] }
      edges[row[0]] = Edge.new(id: row[0], vertices: vs)
    end
    edges
  end

  def self.load_edge_paths(filename, edges, vertices)
    rows = CSV.read(filename, col_sep: "\t")
    edge_paths = [[]]
    rows.each do |row|
      edge = edges[row[0]]
      next if edge.nil?

      signal_from = row[16]
      if signal_from == 'Controller'
        edge.signal_from = vertices[row[17].to_i]
      else
        prev_edge = edges[row[16]]
        edge.signal_from = prev_edge
        prev_edge.signal_to = edge
      end

      edge_paths.last << edge
      if row[18] == 'Terminates'
        edge_paths << []
      end
    end
    edge_paths
  end

  attr_accessor :id, :vertices, :signal_to, :signal_from, :strips
end

class EdgeStrip
  def initialize(id:, vertices:, edge_id:)
    @id = id
    @vertices = vertices
    @edge_id = edge_id
  end

  def num_leds
    @num_leds ||= length * Edge::LEDS_PER_MICRON
  end

  def max_current
    @max_current ||= num_leds * MAX_CURRENT_PER_LED
  end

  def length
    @length ||= vertices[1].distance(vertices[0])
  end

  attr_accessor :id, :vertices, :edge_id
end

