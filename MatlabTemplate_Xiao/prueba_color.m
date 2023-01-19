function [y] = prueba_color(x,y,z,tiempo,a,b)

y = plot(tiempo(a:b),x(a:b),'b',tiempo(a:b),y(a:b),'r',tiempo(a:b),z(a:b),'g','LineWidth',1.5);
        
    
end
