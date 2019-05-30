schema HVACState
    Float32 energy_cost,
    Float32 hour,
    Float32 outdoor_temperature,
    Float32 occupancy,
    Float32 air_recycled
end
 
schema HVACAction
    Float32{0:1} heater_command,
    Float32{0:1} air_recycling_damper_command
end
 
schema HVACConfig
    Float32 day_of_year
end
 
concept hvac_controller is estimator
   predicts (HVACAction)
   follows input(HVACState)
   feeds output
end
 
simulator hvac_simulator(HVACConfig)
    action (HVACAction)
    state (HVACState)
end
 
curriculum my_curriculum
    train hvac_controller
    using algorithm TRPO
    with simulator hvac_simulator
    objective temperature_energy_and_air_quality
        lesson my_first_lesson
            configure
               constrain day_of_year with Float32{0.0:365.0}
            until
                maximize temperature_energy_and_air_quality
end
 