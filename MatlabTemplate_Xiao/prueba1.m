WS_ac_x = datos_finales_sensor.ax_g_;
WS_ac_y = -1*(datos_finales_sensor.ay_g_);
WS_ac_z = -1*(datos_finales_sensor.az_g_);

%%w = prueba_black(WS_ac_x,WS_ac_y,WS_ac_z,time_vector1_sensor);
    while i < length(time_vector1_sensor)
        plot(time_vector1_sensor(1:i),WS_ac_x(1:i),time_vector1_sensor(1:i),WS_ac_y(1:i),time_vector1_sensor(1:i),WS_ac_z(1:i)); 
        i=i+1;
        pause(0.1);
    end

    