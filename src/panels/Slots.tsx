import { FC } from 'react';
import { NavIdProps, Panel, PanelHeader, Group, Header, Div, Placeholder } from '@vkontakte/vkui';

export interface SlotsProps extends NavIdProps {}

export const Slots: FC<SlotsProps> = ({ id }) => {
  return (
    <Panel id={id}>
      <PanelHeader>
        <div style={{ width: '100%', textAlign: 'center' }}>Слоты</div>
      </PanelHeader>
      <Group header={<Header>Доступные слоты</Header>}>
        <Div>
          <Placeholder>Пусто — здесь будут настраиваться свободные слоты.</Placeholder>
        </Div>
      </Group>
    </Panel>
  );
};

export default Slots;
