import { useCallback, useRef } from 'react';
import { useFocusEffect } from '@react-navigation/native';

// Re-ejecuta `refresh` cada vez que la pantalla recupera el foco
// (p. ej. volver de "Editar" a "Mis publicaciones" tras guardar o eliminar),
// pero se salta el primer foco (ahí el useEffect inicial ya carga los datos).
export default function useRefreshOnFocus(refresh) {
  const firstRun = useRef(true);
  useFocusEffect(
    // eslint-disable-next-line react-hooks/exhaustive-deps
    useCallback(() => {
      if (firstRun.current) {
        firstRun.current = false;
        return;
      }
      refresh();
    }, [refresh])
  );
}
