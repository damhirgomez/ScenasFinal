function [y] = prueba_black(x,y,z,tiempo)
i=1;
    while i < length(tiempo)
        y = plot(tiempo(1:i),x(1:i),tiempo(1:i),y(1:i),tiempo(1:i),z(1:i)); 
        i=i+1;
        pause(0.1);
    end
end




